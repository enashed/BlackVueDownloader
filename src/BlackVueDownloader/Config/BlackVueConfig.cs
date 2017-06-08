using System;
using System.Configuration;

namespace BlackVueDownloader.Config
{
    public class BlackVueConfig
    {
        public string CameraUrl => ConfigurationManager.AppSettings["CameraUrl"];

        public string OutputFolder => ConfigurationManager.AppSettings["OutputFolder"];

        public TimeSpan InitialDelay => TimeSpan.FromSeconds(10);

        public TimeSpan RepeatDelay => TimeSpan.FromMinutes(5);
    }
}
