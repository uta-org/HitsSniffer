using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HitsSniffer.Controller.Data;
using Console = Colorful.Console;

namespace HitsSniffer.Controller
{
    public static class SqlPool
    {
        public static ConcurrentQueue<SqlRequest> Requests { get; }
        private static bool IsWorking;

        static SqlPool()
        {
            Requests = new ConcurrentQueue<SqlRequest>();
        }

        public static void StartWorking()
        {
            IsWorking = true;
            Task.Factory.StartNew(InternalStartWorking);
        }

        private static void InternalStartWorking()
        {
            do
            {
                bool success = Requests.TryDequeue(out var request);

                if (success)
                {
                    ProcessRequest(request);
                }
                else
                {
                    Console.WriteLine("There was a problem dequeueing a request!", Color.Red);
                    Thread.Sleep(100);
                }
            }
            while (IsWorking);
            // Requests.Count > 0 &&
        }

        private static void ProcessRequest(SqlRequest request)
        {
        }

        public static void StopWorking()
        {
            IsWorking = false;
        }
    }
}