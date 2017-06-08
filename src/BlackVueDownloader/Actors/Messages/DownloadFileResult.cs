namespace BlackVueDownloader.Actors.Messages
{
    class DownloadFileResult
    {
        public DownloadFileResult(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}
