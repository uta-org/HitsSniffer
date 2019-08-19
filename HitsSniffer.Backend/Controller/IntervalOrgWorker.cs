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

                // Create a instance of the OrgData class, setting all the needed props
                // Then, save it on the DB
            }
        }

        private static void GetWhitelistedUrlsFromDatabase()
        {
            // Get organizations names and add it prefix

            // TODO
            // For this, we will need an IEnumerator with all the records from the repository table
            // Then, foreach record we will get the linked org/user (name) for the repository
            // Then, we will form the complete url
        }
    }
}