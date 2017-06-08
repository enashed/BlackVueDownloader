using Akka.Actor;
using BlackVueDownloader.Config;
using System.Net.Http;
using BlackVueDownloader.Actors.Messages;
using System;
using System.Text.RegularExpressions;
using System.IO;
using Akka.Event;

namespace BlackVueDownloader.Actors
{
    internal class Downloader : ReceiveActor
    {
        private readonly BlackVueConfig _config;
        private readonly HttpClient _httpClient;

        public Downloader(BlackVueConfig config)
        {
            _config = config;

            _httpClient = new HttpClient { BaseAddress = new Uri(_config.CameraUrl) };

            Receive<DownloadIndex>(msg => HandleDownloadIndex());

            Receive<DownloadFile>(msg => HandleDownloadFile(msg));
        }

        private void HandleDownloadIndex()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/blackvue_vod.cgi");

            try
            {

                using (var httpResponse = _httpClient.SendAsync(request).Result)
                {
                    var response = httpResponse.Content.ReadAsStringAsync().Result;

                    SaveTextFile(Path.Combine(_config.OutputFolder, "logs"), DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt", response);

                    Sender.Tell(new DownloadIndexResult(response));
                }
            }
            catch(Exception exception)
            {
                Context.GetLogger().Error(exception, "Error downloading Index file");
            }
        }

        private void SaveTextFile(string directory, string fileName, string contents)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, fileName);

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, contents);
            }
        }

        private void HandleDownloadFile(DownloadFile msg)
        {
            // Video file
            DownloadFile(msg.FileName);

            // Thumbnail
            if (msg.FileName.EndsWith(".mp4"))
            {
                DownloadFile(msg.FileName.Replace(".mp4", ".thm"));
            }

            if (msg.FileName.EndsWith("F.mp4"))
            {
                // GPS File
                DownloadFile(msg.FileName.Replace("F.mp4", ".gps"), false);

                // Accelorometer file
                DownloadFile(msg.FileName.Replace("F.mp4", ".3gf"));
            }
        }

        private void DownloadFile(string url, bool isBinary = true)
        {
            var regex = new Regex("/Record/(?<date>\\d+)_(?<time>\\d+)_(?<mode>.*)\\.(?<ext>.*)");
            var match = regex.Match(url);
            var date = match.Groups["date"].Value;
            var time = match.Groups["time"].Value;
            var mode = match.Groups["mode"].Value;
            var ext = match.Groups["ext"].Value;

            var directoryName = Path.Combine(_config.OutputFolder, date);
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
            
            var fileName = Path.Combine(directoryName, $"{date}_{time}_{mode}.{ext}");
            var retryCount = 0;

            while (true)
            {
                try
                {
                    if (!File.Exists(fileName))
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);

                        using (var httpResponse = _httpClient.SendAsync(request).Result)
                        {
                            if (!httpResponse.IsSuccessStatusCode) break;

                            if (isBinary)
                            {
                                File.WriteAllBytes(fileName, httpResponse.Content.ReadAsByteArrayAsync().Result);
                            }
                            else
                            {
                                File.WriteAllText(fileName, httpResponse.Content.ReadAsStringAsync().Result);
                            }
                        }
                    }

                    break;
                }
                catch (Exception exception)
                {
                    Context.GetLogger().Error(exception, "Error downloading {0}", url);

                    if (retryCount++ > 2) break;
                }
            }
        }
    }
}
