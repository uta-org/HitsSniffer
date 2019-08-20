using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using HitsSniffer.Controller;
using HitsSniffer.Controller.Interfaces;
using Console = Colorful.Console;

namespace HitsSniffer
{
    // TODO: Search how to awake this process when a reboot is done in Linux
    // TODO: Display bandwidth usage

    internal sealed class Program
    {
        private static List<IWorker> Workers = new List<IWorker>
        {
            IntervalOrgWorker.Instance,
            IntervalHitsWorker.Instance,
            IntervalRepoWorker.Instance,
            IntervalUserWorker.Instance
        };

        private static void Main(string[] args)
        {
            SqlWorker.OpenConnection();

            handler = ConsoleEventCallback;
            SetConsoleCtrlHandler(handler, true);

            foreach (var worker in Workers)
            {
                try
                {
                    worker.StartWorking();
                }
                catch
                {
                    Console.WriteLine($"WARNING: Not implemented worker of type '{worker.GetType().Name}' method 'StartWorking'...", Color.Yellow);
                }
            }

            Console.ReadKey(true);
        }

        private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                // Console exiting
                SqlWorker.Release();

                foreach (var worker in Workers)
                {
                    try
                    {
                        worker.FinishWorking();
                    }
                    catch
                    {
                        Console.WriteLine($"WARNING: Not implemented worker of type '{worker.GetType().Name}' method 'FinishWorking'...", Color.Yellow);
                    }
                }
            }

            return false;
        }

        private static ConsoleEventDelegate handler; // Keeps it from getting garbage collected

        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }
}