using System.Threading;

namespace HitsSniffer.Controller
{
    public sealed class IntervalUserWorker : BaseWorker<IntervalUserWorker>
    {
        protected override Timer Timer { get; set; }

        protected override string[] WhitelistedUrls { get; set; }

        protected override void TimerCallback(object state)
        {
            throw new System.NotImplementedException();
        }

        public override void StartWorking()
        {
            throw new System.NotImplementedException();
        }

        public override void FinishWorking()
        {
            throw new System.NotImplementedException();
        }
    }
}