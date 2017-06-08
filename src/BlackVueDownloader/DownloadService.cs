using Akka.Actor;
using BlackVueDownloader.Actors;
using BlackVueDownloader.Actors.Messages;
using BlackVueDownloader.Config;

namespace BlackVueDownloader
{
    internal class DownloadService
    {
        private readonly ActorSystem _system;
        private readonly IActorRef _coordinator;
        private readonly BlackVueConfig _config;

        public DownloadService()
        {
            _config = new BlackVueConfig();
            _system = ActorSystem.Create("BlackVue");
            _coordinator = _system.ActorOf(Props.Create(() => new DownloadCoordinator(_config)));
        }

        public void Start()
        {
            _system.Scheduler.ScheduleTellRepeatedly(_config.InitialDelay, _config.RepeatDelay, _coordinator, StartDownload.Instance, ActorRefs.Nobody);
        }

        public void Stop()
        {
            _system.Terminate().Wait();
        }
    }
}
