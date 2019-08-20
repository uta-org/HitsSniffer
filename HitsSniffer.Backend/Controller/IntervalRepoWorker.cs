using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HitsSniffer.Model;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using uzLib.Lite.Extensions;
using static HitsSniffer.Controller.DriverWorker;

namespace HitsSniffer.Controller
{
    public sealed class IntervalRepoWorker : BaseWorker<IntervalRepoWorker>
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

                // Create a instance of the OrgData class, setting all the needed props
                // Then, save it on the DB

                var tabs = html.GetNodesByClass("reponav").Where(child => child.Name.ToLowerInvariant() == "a");
                var summary = html.GetNodesByClass("numbers-summary");
                var socialCount = html.GetNodesByClass("js-social-count");

                const string textEmphasized = "text-emphasized";

                string name = whitelistedUrl.Split('/').Last();

                var repoData = new RepoData
                {
                    Name = name,
                    Commits = GetCountFrom(summary, textEmphasized, 0),
                    Projects = GetCountFrom(tabs, 3),
                    Branches = GetCountFrom(summary, textEmphasized, 1),
                    Contributors = GetCountFrom(summary, textEmphasized, 3),
                    Forks = GetCountFrom(socialCount, 2, false),
                    Issues = GetCountFrom(tabs, 1),
                    Stars = GetCountFrom(socialCount, 1, false),
                    Pulls = GetCountFrom(tabs, 2),
                    Watchers = GetCountFrom(socialCount, 0, false),
                    Releases = GetCountFrom(summary, textEmphasized, 2),
                    LastCommit = DateTime.MinValue, // TODO
                    OrgId = null, // TODO
                    UserId = null, // TODO
                    Date = DateTime.Now
                };

                repoData.DoQuery();
            }
        }

        private static int GetCountFrom(IEnumerable<HtmlNode> nodes, string name, int index)
        {
            var node = nodes.ElementAt(index);
            return int.Parse(node.GetNodeByClass(name).InnerText);
        }

        private static int GetCountFrom(IEnumerable<HtmlNode> nodes, int index, bool useGlobalSelector = true)
        {
            var node = nodes.ElementAt(index);
            return int.Parse((useGlobalSelector ? node.GetNodeByClass(countSelector) : node).InnerText);
        }

        protected override void GetWhitelistedUrlsFromDatabase()
        {
            // For this, we will need an IEnumerator with all the records from the repository table
            // Then, foreach record we will get the linked org/user (name) for the repository
            // Then, we will form the complete url

            var localList = new List<string>();

            SqlWorker.IterateRecords<RepoData>(reader =>
            {
                string ownerName = string.Empty;
                int? ownerId = (int?)reader["org_id"] ?? (int?)reader["user_id"];

                if (!ownerId.HasValue)
                    throw new Exception();

                bool isOrg = ((int?)reader["org_id"]).HasValue;

                if (isOrg)
                    SqlWorker.DoSelect<OrgData>(
                        _reader => ReaderCallback(_reader, out ownerName),
                        "name",
                        "id = @id",
                        "date",
                        "1",
                        false,
                        new Tuple<string, object>("@id", ownerId.Value));
                else
                    SqlWorker.DoSelect<UserData>(
                        _reader => ReaderCallback(_reader, out ownerName),
                        "name",
                        "id = @id",
                        "date",
                        "1",
                        false,
                        new Tuple<string, object>("@id", ownerId.Value));

                string repoName = reader["name"].ToString();

                localList.Add(string.Format(TemplateUrl, $"{ownerName}/{repoName}"));
            });

            WhitelistedUrls = localList.Select(item => string.Format(TemplateUrl, item)).ToArray();
        }

        private void ReaderCallback(MySqlDataReader obj, out string ownerName)
        {
            ownerName = obj["name"].ToString();
        }
    }
}