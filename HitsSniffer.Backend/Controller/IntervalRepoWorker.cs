using System.Threading;

namespace HitsSniffer.Controller
{
    public sealed class IntervalRepoWorker : BaseWorker<IntervalRepoWorker>
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

        // TODO: DONT DELETE
        public override void FinishWorking()
        {
            throw new System.NotImplementedException();

            // Get organizations names and add it prefix

            // TODO
            // For this, we will need an IEnumerator with all the records from the repository table
            // Then, foreach record we will get the linked org/user (name) for the repository
            // Then, we will form the complete url
        }
    }
}