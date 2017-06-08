using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace BlackVueDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 
            {
                x.Service<DownloadService>(s =>
                {
                    s.ConstructUsing(name => new DownloadService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();
                x.StartAutomaticallyDelayed();
                x.SetDisplayName("BlackVue Downloader");
                x.SetServiceName("BlackVue Downloader");
                x.SetDescription("BlackVue Downloader");
            });
        }
    }
}
