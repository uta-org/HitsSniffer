﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HitsSniffer.Model;
using HtmlAgilityPack;
using uzLib.Lite.Extensions;
using static HitsSniffer.Controller.DriverWorker;

namespace HitsSniffer.Controller.Intervals
{
    public sealed class IntervalOrgWorker : BaseWorker<IntervalOrgWorker>
    {
        protected override Timer Timer { get; set; }

        protected override string[] WhitelistedUrls { get; set; }

        private const string countSelector = "js-profile-repository-count";

        public override void StartWorking()
        {
            PrepareDriver();
            GetWhitelistedUrlsFromDatabase();
        }

        public override void FinishWorking()
        {
            CloseDriver();
        }

        protected override void TimerCallback(object state)
        {
            // TODO: Do parallel work
            foreach (var whitelistedUrl in WhitelistedUrls)
            {
                var html = PrepareSource(whitelistedUrl);

                // Create a instance of the OrgData class, setting all the needed props
                // Then, save it on the DB

                var tabs = html.GetNodesByClass("pagehead-tabs-item");

                // html.GetNodeByClass("org-name").InnerText
                var orgData = new OrgData
                {
                    Name = whitelistedUrl.Split('/').Last(),
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
            // TODO: If none Org is added in the database then this will be empty, we need to add this from the dumped data from the socket
            // To fix it, we need to ensure that in every 10 minutes cycle we add the needed data

            var localList = new List<string>();

            SqlWorker.IterateRecords<OrgData>(reader =>
            {
                localList.Add(reader["name"].ToString());
            });

            WhitelistedUrls = localList.Select(item => string.Format(TemplateUrl, item)).ToArray();
        }
    }
}