using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HitsSniffer.Model;
using HtmlAgilityPack;
using uzLib.Lite.Extensions;
using static HitsSniffer.Controller.DriverWorker;

namespace HitsSniffer.Controller.Intervals
{
    public sealed class IntervalUserWorker : BaseWorker<IntervalUserWorker>
    {
        protected override Timer Timer { get; set; }

        protected override string[] WhitelistedUrls { get; set; }

        private const string countSelector = "Counter";

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

                var tabs = html.GetNodeByClass("UnderlineNav-body").GetNodesByName("a");

                // Create a instance of the OrgData class, setting all the needed props
                // Then, save it on the DB

                string name = whitelistedUrl.Split('/').Last();
                int commitNum = TrackCommits(html, name, out int lastYearCommitNum);

                var userData = new UserData
                {
                    Name = name,
                    Projects = GetCountFrom(tabs, 2),
                    Repositories = GetCountFrom(tabs, 1),
                    Commits = commitNum,
                    CommitsLastYear = lastYearCommitNum,
                    Followers = GetCountFrom(tabs, 4),
                    Following = GetCountFrom(tabs, 5),
                    StarsGiven = GetCountFrom(tabs, 3),
                    Date = DateTime.Now
                };

                userData.DoQuery();
            }
        }

        private int TrackCommits(HtmlNode html, string username, out int lastYearCommits)
        {
            // First, check if we have any record for this user on the database
            bool existsUser = SqlWorker.DoExistsTransaction<UserData>(username).HasValue;
            int totalCommits;

            var years = html
                .OwnerDocument
                .GetElementbyId("year-list-container")
                .GetNodesByName("li")
                .Select(li => string.Format(TemplateUrl, li.FirstChild.GetAttributeValue("href", string.Empty).Substring(1)));

            // (In both cases) Then we will track only the last year

            string firstDayOfCurrentYear = $"{DateTime.Now:yyyy}-01-01";
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            string url = $"{username}/?tab=overview&from={firstDayOfCurrentYear}&to={today}";
            lastYearCommits = GetContributionsFromSource(url);

            if (!existsUser)
            {
                // If it's the first time we track it, then we will track all the years

                totalCommits = years.Select(GetContributionsFromSource).Sum();
            }
            else
            {
                // TODO: Check this
                int lastTotalCommits = 0;
                int lastRecordedLastYearCommit = 0;

                SqlWorker.DoSelect<UserData>(reader =>
                {
                    lastTotalCommits = int.Parse(reader["commits"].ToString());
                    lastRecordedLastYearCommit = int.Parse(reader["commits_last_year"].ToString());
                },
                    "*",
                    "name = @name",
                    "date",
                    "1",
                    false,
                    new Tuple<string, object>("@name", username));

                int _commits = lastTotalCommits + (lastYearCommits - lastRecordedLastYearCommit);
                totalCommits = _commits;
            }

            return totalCommits;
        }

        private int GetContributionsFromSource(string url)
        {
            var html = PrepareSource(url, false);
            return html.GetYearlyContributions();
        }

        private static int GetCountFrom(IEnumerable<HtmlNode> nodes, int index)
        {
            var node = nodes.ElementAt(index);

            return int.Parse(node.GetNodeByClass(countSelector).InnerText);
        }

        protected override void GetWhitelistedUrlsFromDatabase()
        {
            var localList = new List<string>();

            SqlWorker.IterateRecords<UserData>(reader =>
            {
                localList.Add(reader["name"].ToString());
            });

            WhitelistedUrls = localList.Select(item => string.Format(TemplateUrl, item)).ToArray();
        }
    }
}