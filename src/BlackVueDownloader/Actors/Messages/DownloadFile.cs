namespace BlackVueDownloader.Actors.Messages
{
    class DownloadFile
    {
        public DownloadFile(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}
