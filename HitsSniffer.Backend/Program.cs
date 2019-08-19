using System.Runtime.InteropServices;
using HitsSniffer.Controller;
using Console = Colorful.Console;

namespace HitsSniffer
{
    // TODO: Search how to awake this process when a reboot is done in Linux
    // TODO: Display bandwidth usage

    internal sealed class Program
    {
        private static IntervalHitsWorker HitsWorker { get; } = IntervalHitsWorker.Instance;
        private static IntervalOrgWorker OrgWorker { get; } = IntervalOrgWorker.Instance;

        private static void Main(string[] args)
        {
            SqlWorker.OpenConnection();

            handler = ConsoleEventCallback;
            SetConsoleCtrlHandler(handler, true);

            HitsWorker.StartWorking();
            OrgWorker.StartWorking();

            Console.ReadKey(true);
        }

        private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                // Console exiting
                SqlWorker.Release();

                OrgWorker.FinishWorking();
            }

            return false;
        }

        private static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected

        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }
}