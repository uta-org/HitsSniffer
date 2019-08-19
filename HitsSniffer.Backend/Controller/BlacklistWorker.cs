using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using uzLib.Lite.Extensions;
using static HitsSniffer.Controller.DriverWorker;

namespace HitsSniffer.Controller
{
    public static class BlacklistWorker
    {
        public enum Type
        {
            Repository,
            User,
            Organization
        }

        private static string RepositoryBlacklistPath { get; } =
            Path.Combine(Environment.CurrentDirectory, "repo_blacklist.json");

        private static string UserBlacklistPath { get; } =
            Path.Combine(Environment.CurrentDirectory, "user_blacklist.json");

        private static string OrganizationBlacklistPath { get; } =
            Path.Combine(Environment.CurrentDirectory, "org_blacklist.json");

        static BlacklistWorker()
        {
            if (File.Exists(RepositoryBlacklistPath))
                RepositoryBlacklist =
                    JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(RepositoryBlacklistPath));

            if (File.Exists(UserBlacklistPath))
                UserBlacklist =
                    JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(UserBlacklistPath));

            if (File.Exists(OrganizationBlacklistPath))
                OrganizationBlacklist =
                    JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(OrganizationBlacklistPath));
        }

        public static HashSet<string> RepositoryBlacklist { get; } = new HashSet<string>();
        public static HashSet<string> UserBlacklist { get; } = new HashSet<string>();
        public static HashSet<string> OrganizationBlacklist { get; } = new HashSet<string>();

        private static bool IsRepositoryChanged;
        private static bool IsUserChanged;
        private static bool IsOrganizationChanged;

        // Create a blacklist worker, where users/repos/orgs without enough activity will not be tracked
        // Conditions must be:
        // For orgs and repositories, activity in the last 10 days
        // For orgs, at least 5 repos
        // For users, activity in the last month && at least 50 commits in the last year
        // For repos, at least 10 commits

        public static bool IsRepositoryListable(string path)
        {
            var html = PrepareSourceCode(path);

            var parsedTime = ParseTime(html.GetNodeByClass("commit-tease").ChildNodes[2].ChildNodes[2].FirstChild
                                .GetAttributeValue("datetime", string.Empty));

            int commits = int.Parse(html.GetNodeByClass("commits").GetNodeByClass("text-emphasized").InnerText);

            return DateTime.Now - parsedTime > TimeSpan.FromDays(10) && commits >= 10;
        }

        public static bool IsUserListable(string username)
        {
            var html = PrepareSourceCode(username);

            string currentMonth = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);
            string lastTimelineMonth = html.GetNodeByClass("profile-timeline-month-heading").InnerText;

            string contributionsStr = html.GetNodeByClass("js-yearly-contributions").FirstChild.ChildNodes[1].InnerText;
            int contribsOnLastYear = int.Parse(Regex.Match(contributionsStr, @"\d+").Value);

            return lastTimelineMonth == currentMonth && contribsOnLastYear >= 50;
        }

        public static bool IsOrgListable(string organization)
        {
            var html = PrepareSourceCode(organization);

            var ul = html.GetNodeByClass("repo-list")
                .FirstChild;

            var lastUpdate =
                ParseTime(ul.FirstChild.ChildNodes.Last()
                    .GetNodeByName("relative-time")
                    .GetAttributeValue("datetime", string.Empty));

            // TODO: Debug
            return DateTime.Now - lastUpdate > TimeSpan.FromDays(10) && ul.ChildNodes.Count > 5;
        }

        private static DateTime ParseTime(string timeStr)
        {
            return DateTime.ParseExact(timeStr,
                "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal);
        }

        private static HtmlNode PrepareSourceCode(string name)
        {
            string url = string.Format(TemplateUrl, name);
            string source;

            using (var wc = new WebClient())
                source = wc.DownloadString(url);

            var doc = new HtmlDocument();
            doc.Load(source);

            return doc.DocumentNode;
        }

        public static bool IsValid(string value, Type type)
        {
            switch (type)
            {
                case Type.Organization:
                    return !OrganizationBlacklist.Contains(value);

                case Type.Repository:
                    return !RepositoryBlacklist.Contains(value);

                case Type.User:
                    return !UserBlacklist.Contains(value);
            }

            return false;
        }

        public static void Add(string value, Type type)
        {
            switch (type)
            {
                case Type.Organization:
                    OrganizationBlacklist.Add(value);
                    IsOrganizationChanged = true;
                    break;

                case Type.Repository:
                    RepositoryBlacklist.Add(value);
                    IsRepositoryChanged = true;
                    break;

                case Type.User:
                    UserBlacklist.Add(value);
                    IsUserChanged = true;
                    break;
            }

            Save();
        }

        public static void Save()
        {
            if (IsRepositoryChanged)
                File.WriteAllText(RepositoryBlacklistPath, JsonConvert.SerializeObject(RepositoryBlacklist));

            if (IsOrganizationChanged)
                File.WriteAllText(OrganizationBlacklistPath, JsonConvert.SerializeObject(OrganizationBlacklist));

            if (IsUserChanged)
                File.WriteAllText(UserBlacklistPath, JsonConvert.SerializeObject(UserBlacklistPath));
        }
    }
}