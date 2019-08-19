using System.Threading;
using HitsSniffer.Controller.Interfaces;
using uzLib.Lite.Core;
using static HitsSniffer.Controller.DriverWorker;

namespace HitsSniffer.Controller
{
    public class IntervalOrgWorker : Singleton<IntervalOrgWorker>, IWorker
    {
        public int IntervalMs { get; } = 10 * 60 * 1000;
        public Timer Timer { get; private set; }

        private static string[] WhitelistedUrls;

        public void StartWorking()
        {
            Timer = new Timer(TimerCallback, null, 0, IntervalMs);
        }

        public void FinishWorking()
        {
            CloseDriver();
        }

        private void TimerCallback(object state)
        {
            PrepareDriver();
            GetWhitelistedUrlsFromDatabase();

            // TODO: Do parallel work
            foreach (var whitelistedUrl in WhitelistedUrls)
            {
                Driver.Navigate().GoToUrl(whitelistedUrl);
            }
        }

        private static void GetWhitelistedUrlsFromDatabase()
        {
        }
    }
}