﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HitsSniffer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uzLib.Lite.Extensions;
using uzLib.Lite.ExternalCode.Extensions;
using Console = Colorful.Console;

namespace HitsSniffer.Controller.Intervals
{
    public sealed class IntervalHitsWorker : BaseWorker<IntervalHitsWorker>
    {
        private static string RegexPattern { get; } = @"\{.+?\}";
        private static string HitUrl { get; } = "http://hits.dwyl.io/socket.io/?EIO=3&transport=polling&sid={0}";
        private static string ContentType { get; } = "text/plain; charset=UTF-8";
        private static string UserAgent { get; } = "curl/7.65.3";
        private static string AcceptHeader { get; } = "*/*";

        protected override Timer Timer { get; set; }

        protected override string[] WhitelistedUrls { get; set; }

        private static string CurrentSID;
        private static DateTime InternalTimer;
        private static int PingInterval;
        private static int PingTimeout;

        public override int IntervalMs { get; } = 2000;

        public override void StartWorking()
        {
            CurrentSID = GetSID(out PingInterval, out PingTimeout);
            Timer = new Timer(TimerCallback, null, 0, DEBUG ? IntervalMs : PingInterval);
        }

        protected override void TimerCallback(object state)
        {
            if (string.IsNullOrEmpty(CurrentSID) || HasTimePassed())
                CurrentSID = GetSID();

            if (string.IsNullOrEmpty(CurrentSID))
            {
                Console.WriteLine("There was an error receiving the SID!", Color.Red);
                return;
            }

            var data = GetData(CurrentSID).ToList();

            if (data.IsNullOrEmpty())
            {
                Console.WriteLine($"Wrong request made at {DateTime.Now} with SID '{CurrentSID}'!", Color.Red);
                return;
            }

            data.ForEach(Console.WriteLine);

            foreach (var hitData in data)
            {
                //Task.Factory.StartNew(hitData.DoQuery);
                hitData.DoQuery();
            }
        }

        private string GetSID()
        {
            return GetSID(out int interval, out int timeout);
        }

        private string GetSID(out int interval, out int timeout)
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

        private IEnumerable<HitData> GetData(string sid)
        {
            // TODO: If exception occurred then use a new sid
            // TODO: Create a system to identify missing hits

            string url = string.Format(HitUrl, sid);
            string json;

            try
            {
                json = url.MakeRequest(ContentType, UserAgent, AcceptHeader, true);
            }
            catch
            {
                string _sid = (string)CurrentSID.Clone();

                CurrentSID = GetSID();
                Console.WriteLine($"Error occurred while making request! (Changing SID from '{_sid}' to '{CurrentSID}')", Color.Red);

                string newUrl = string.Format(HitUrl, CurrentSID);
                json = newUrl.MakeRequest(ContentType, UserAgent, AcceptHeader);
            }

            if (!Regex.IsMatch(json, RegexPattern))
            {
                if (DEBUG)
                    Console.WriteLine($"Not matching:\n{json}");

                yield break;
            }

            var matches = Regex.Matches(json, RegexPattern);

            foreach (Match match in matches)
            {
                var obj = JsonConvert.DeserializeObject(match.Value) as JObject;

                if (obj?["hit"] != null)
                {
                    string data = obj["hit"].ToObject<string>();
                    yield return new HitData(data, sid).TransformData();
                }
            }
        }

        private bool HasTimePassed()
        {
            bool hasTimePassed = DateTime.UtcNow - InternalTimer > TimeSpan.FromSeconds(PingTimeout);

            if (hasTimePassed)
                InternalTimer = DateTime.UtcNow;

            return hasTimePassed;
        }
    }
}