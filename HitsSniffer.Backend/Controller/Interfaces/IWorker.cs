using System.Threading;

namespace HitsSniffer.Controller.Interfaces
{
    public interface IWorker
    {
        Timer Timer { get; }

        void StartWorking();

        void FinishWorking();
    }
}