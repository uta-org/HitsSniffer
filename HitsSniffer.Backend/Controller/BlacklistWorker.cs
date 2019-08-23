using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using HitsSniffer.Model;
using HitsSniffer.Model.Interfaces;
using HtmlAgilityPack;
using Newtonsoft.Json;
using uzLib.Lite.Extensions;
using static HitsSniffer.Controller.DriverWorker;
using Console = Colorful.Console;

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

            DateTime parsedTime;
            try
            {
                parsedTime = ParseTime(html.GetNodeByClass("commit-tease").GetElementChildNode(2).GetElementChildNode(2).FirstChild()
                                    .GetAttributeValue("datetime", string.Empty));
            }
            catch
            {
                Console.WriteLine($"Cannot retrieve the last commit at this time for '{path}'!", Color.Yellow);
                return false;
            }

            int commits = int.Parse(html.GetNodeByClass("commits").GetNodeByClass("text-emphasized").InnerText);

            var dateDiff = DateTime.Now - parsedTime;
            bool isValid = dateDiff < TimeSpan.FromDays(10) && commits >= 10;

            if (!isValid)
                NotifyInvalidSource<UserData>(path,
                    new[] { "Time From Last Commit", dateDiff.TotalDays.ToString("F2", CultureInfo.InvariantCulture) },
                    new[] { "Commits", commits.ToString() });

            return isValid;
        }

        public static bool IsUserListable(string username)
        {
            var html = PrepareSourceCode(username);

            string currentMonth = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);
            string lastTimelineMonth = html.GetNodeByClass("profile-timeline-month-heading").InnerText;

            int contribsOnLastYear = html.GetYearlyContributions();

            bool isValid = lastTimelineMonth == currentMonth && contribsOnLastYear >= 50;

            if (!isValid)
                NotifyInvalidSource<UserData>(username,
                    new[] { "Has Committed On The Last Month", (lastTimelineMonth == currentMonth).ToString() },
                    new[] { "Contributions On The Last Year", contribsOnLastYear.ToString() });

            return isValid;
        }

        public static bool IsOrgListable(string organization)
        {
            var html = PrepareSourceCode(organization);

            var ul = html.GetNodeByClass("repo-list")
                .FirstChild();

            var lastUpdate =
                ParseTime(ul.FirstChild().LastChild()
                    .GetNodeByName("relative-time")
                    .GetAttributeValue("datetime", string.Empty));

            int numberOfRepos = ul.GetElementChildNodes().Count();
            var dateDiff = DateTime.Now - lastUpdate;

            bool isValid = dateDiff < TimeSpan.FromDays(10) && numberOfRepos > 5;

            if (!isValid)
                NotifyInvalidSource<OrgData>(organization,
                    new[] { "Last Update", dateDiff.TotalDays.ToString("F2", CultureInfo.InvariantCulture) },
                    new[] { "Repository Count", numberOfRepos.ToString() });

            return isValid;
        }

        private static void NotifyInvalidSource<T>(string identifier, params string[][] values)
            where T : IPrimaryKey
            => NotifyInvalidSource<T>(identifier, Color.Yellow, values);

        private static void NotifyInvalidSource<T>(string identifier, Color color = default, params string[][] values)
        where T : IPrimaryKey
        {
            string simplifiedValues = string.Join(", ", values.Select(value => $"{value[0]}: {value[1]}"));

            T testInstance = Activator.CreateInstance<T>();
            Console.WriteLine($"The {testInstance.Identifier()} '{identifier}' doesn't meet the criteria. ({simplifiedValues})", color);
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
            doc.LoadHtml(source);

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