using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uzLib.Lite.Extensions;
using uzLib.Lite.ExternalCode.Extensions;
using Console = Colorful.Console;

namespace HitsSniffer
{
    internal class Program
    {
        private static string RegexPattern { get; } = @"\{.+?\}";
        private static string HitUrl { get; } = "http://hits.dwyl.io/socket.io/?EIO=3&transport=polling&sid={0}";
        private static string ContentType { get; } = "text/plain; charset=UTF-8";
        private static string UserAgent { get; } = "curl/7.65.3";
        private static string AcceptHeader { get; } = "*/*";

        private static string LastSID;
        private static DateTime Timer;
        private static int PingInterval;
        private static int PingTimeout;

        private const bool DEBUG = true;
        private static int IntervalDebugMS = 2000;

        private static void Main(string[] args)
        {
            LastSID = GetSID(out PingInterval, out PingTimeout);

            var timer = new Timer(state => TimerCallback(LastSID, state), null, 0, DEBUG ? IntervalDebugMS : PingInterval);
            Console.ReadKey(true);
        }

        private static void TimerCallback(string sid, object state)
        {
            if (string.IsNullOrEmpty(LastSID) || HasTimePassed())
                LastSID = GetSID();

            if (string.IsNullOrEmpty(sid))
            {
                Console.WriteLine("There was an error receiving the SID!", Color.Red);
                return;
            }

            var data = GetData(sid).ToList();

            if (data.IsNullOrEmpty())
            {
                Console.WriteLine($"Wrong request made at {DateTime.Now} with SID '{sid}'!", Color.Red);
                return;
            }

            data.ForEach(Console.WriteLine);
        }

        private static string GetSID()
        {
            return GetSID(out int interval, out int timeout);
        }

        private static string GetSID(out int interval, out int timeout)
        {
            const string url = "http://hits.dwyl.io/socket.io/?EIO=3&transport=polling";

            string rawJSON = url.MakeRequest(ContentType, UserAgent, AcceptHeader);

            if (!Regex.IsMatch(rawJSON, RegexPattern))
            {
                interval = -1;
                timeout = -1;

                return string.Empty;
            }

            string json = Regex.Match(rawJSON, RegexPattern).Value;
            var obj = JsonConvert.DeserializeObject(json) as JObject;

            timeout = (obj?["pingTimeout"].ToObject<int>()).GetValue(-1) / 1000;
            interval = (obj?["pingInterval"].ToObject<int>()).GetValue(-1);

            return obj?["sid"].ToObject<string>();
        }

        private static IEnumerable<HitData> GetData(string sid)
        {
            // TODO: If exception occurred then use a new sid
            // TODO: XCrewate a system to identify missing hits

            string url = string.Format(HitUrl, sid);
            string json = url.MakeRequest(ContentType, UserAgent, AcceptHeader);

            if (!Regex.IsMatch(json, RegexPattern))
                yield break;

            var matches = Regex.Matches(json, RegexPattern);

            foreach (Match match in matches)
            {
                var obj = JsonConvert.DeserializeObject(match.Value) as JObject;

                if (obj?["hit"] != null)
                {
                    string data = obj["hit"].ToObject<string>();
                    yield return new HitData(data, sid);
                }
            }
        }

        private static bool HasTimePassed()
        {
            bool hasTimePassed = DateTime.UtcNow - Timer > TimeSpan.FromSeconds(PingTimeout);

            if (hasTimePassed)
                Timer = DateTime.UtcNow;

            return hasTimePassed;
        }
    }

    public class HitData
    {
        public string Data { get; }
        public string SID { get; }

        private HitData()
        {
        }

        public HitData(string data, string sid)
        {
            Data = data;
            SID = sid;
        }

        public override string ToString()
        {
            return $"SID: {SID}" +
                   Environment.NewLine +
                   $"Data: {Data}";
        }
    }
}