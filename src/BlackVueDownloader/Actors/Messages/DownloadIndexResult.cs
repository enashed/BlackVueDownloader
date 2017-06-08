namespace BlackVueDownloader.Actors.Messages
{
    public class DownloadIndexResult
    {
        public DownloadIndexResult(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
