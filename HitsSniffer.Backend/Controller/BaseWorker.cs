using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HitsSniffer.Controller.Interfaces;
using HtmlAgilityPack;
using uzLib.Lite.Core;
using uzLib.Lite.Extensions;
using static HitsSniffer.Controller.DriverWorker;

namespace HitsSniffer.Controller
{
    public abstract class BaseWorker<T> : Singleton<T>, IWorker
        where T : class
    {
        protected const bool DEBUG = true;

        public virtual int IntervalMs { get; } = 10 * 60 * 1000;

        protected abstract Timer Timer { get; set; }
        protected abstract string[] WhitelistedUrls { get; set; }

        protected abstract void TimerCallback(object state);

        public virtual void StartWorking()
        {
            throw new NotImplementedException();
        }

        public virtual void FinishWorking()
        {
            throw new NotImplementedException();
        }

        protected virtual void GetWhitelistedUrlsFromDatabase()
        {
            throw new NotImplementedException();
        }

        protected virtual HtmlNode PrepareSource(string url, bool useDriver = true)
            => GetSourceCode(url, useDriver);

        private static HtmlNode GetSourceCode(string url, bool useDriver)
        {
            if (useDriver)
            {
                Driver.Navigate().GoToUrl(url);
                var source = Driver.PageSource;

                var doc = new HtmlDocument();
                doc.LoadHtml(source);

                return doc.DocumentNode;
            }
            else
            {
                string source;

                using (var wc = new WebClient())
                    source = wc.DownloadString(url);

                var doc = new HtmlDocument();
                doc.Load(source);

                return doc.DocumentNode;
            }
        }
    }

    internal static class BaseWorkerHelper
    {
        internal static int GetYearlyContributions(this HtmlNode html)
        {
            string contributionsStr = html.GetNodeByClass("js-yearly-contributions").FirstChild.ChildNodes[1].InnerText;
            return int.Parse(Regex.Match(contributionsStr, @"\d+").Value);
        }
    }
}