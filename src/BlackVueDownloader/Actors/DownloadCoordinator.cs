using Akka.Actor;
using BlackVueDownloader.Config;
using BlackVueDownloader.Actors.Messages;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Akka.Routing;

namespace BlackVueDownloader.Actors
{
    public class DownloadCoordinator : ReceiveActor
    {
        private readonly BlackVueConfig _config;

        private bool _downloading = false;

        private IActorRef _downloader;

        public DownloadCoordinator(BlackVueConfig config)
        {
            _config = config;

            Ready();
        }

        protected override void PreStart()
        {
            base.PreStart(); 

            SetReceiveTimeout(TimeSpan.FromMinutes(2));

            _downloader = Context.ActorOf(Props.Create(() => new Downloader(_config)).WithRouter(new RoundRobinPool(1)));
        }

        private void Ready()
        {
            Receive<StartDownload>(m => _downloading == false, msg => HandleStartDownload());

            Receive<DownloadIndexResult>(msg => HandleDownloadIndexResult(msg));

            Receive<DownloadFileResult>(msg => HandleDownloadDownloadFile(msg));

            Receive<ReceiveTimeout>(msg => { _downloading = false; });
        }

        private void HandleStartDownload()
        {
            _downloading = true;

            _downloader.Tell(new DownloadIndex());
        }

        private void HandleDownloadIndexResult(DownloadIndexResult msg)
        {
            var regex = new Regex("n:(?<name>.*?),s:", RegexOptions.Multiline);

            var matches = regex.Matches(msg.Content);

            var fileNames = new List<string>();

            foreach (Match match in matches)
            {
                fileNames.Add(match.Groups["name"].Value);
            }

            fileNames = fileNames.OrderBy(x => x.Contains("_E") ? 1 : x.Contains("_N") ? 2 : 3)
                                 .ThenBy(x => x)
                                 .ToList();

            foreach (var fileName in fileNames)
            {
                _downloader.Tell(new DownloadFile(fileName));
            }
        }

        private void HandleDownloadDownloadFile(DownloadFileResult msg)
        {
            // Do Nothing
        }
    }
}
