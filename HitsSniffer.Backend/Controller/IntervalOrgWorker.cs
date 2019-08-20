using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HitsSniffer.Model;
using HtmlAgilityPack;
using uzLib.Lite.Extensions;
using static HitsSniffer.Controller.DriverWorker;

namespace HitsSniffer.Controller
{
    public sealed class IntervalOrgWorker : BaseWorker<IntervalOrgWorker>
    {
        protected override Timer Timer { get; set; }

        protected override string[] WhitelistedUrls { get; set; }

        private const string countSelector = "js-profile-repository-count";

        public override void FinishWorking()
        {
            CloseDriver();
        }

        protected override void TimerCallback(object state)
        {
            PrepareDriver();
            GetWhitelistedUrlsFromDatabase();

            // TODO: Do parallel work
            foreach (var whitelistedUrl in WhitelistedUrls)
            {
                var html = PrepareSource(whitelistedUrl);

                // Create a instance of the OrgData class, setting all the needed props
                // Then, save it on the DB

                var tabs = html.GetNodesByClass("pagehead-tabs-item");

                var orgData = new OrgData
                {
                    Name = html.GetNodeByClass("org-name").InnerText,
                    Members = int.Parse(html.GetNodesByClass("js-profile-tab-count-container").First(node => node.Name.ToLowerInvariant() == "div").GetNodeByClass(countSelector).InnerText),
                    Packages = GetCountFrom(tabs, 1),
                    Projects = GetCountFrom(tabs, 3),
                    Repositories = GetCountFrom(tabs, 0),
                    Teams = GetCountFrom(tabs, 2),
                    Date = DateTime.Now
                };

                orgData.DoQuery();
            }
        }

        private static int GetCountFrom(IEnumerable<HtmlNode> nodes, int index)
        {
            var node = nodes.ElementAt(index);

            return int.Parse(node.GetNodeByClass(countSelector).InnerText);
        }

        protected override void GetWhitelistedUrlsFromDatabase()
        {
            // Get organizations names and add it prefix

            // TODO
            // For this, we will need an IEnumerator with all the records from the repository table
            // Then, foreach record we will get the linked org/user (name) for the repository
            // Then, we will form the complete url
        }
    }
}