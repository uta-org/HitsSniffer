using System;
using System.Threading;
using HitsSniffer.Controller.Interfaces;
using HtmlAgilityPack;
using uzLib.Lite.Core;
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

        protected virtual HtmlNode PrepareSource(string url)
        {
            Driver.Navigate().GoToUrl(url);
            var source = Driver.PageSource;

            var doc = new HtmlDocument();
            doc.LoadHtml(source);

            return doc.DocumentNode;
        }
    }
}