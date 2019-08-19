using System;
using System.Data;
using System.Net;
using HitsSniffer.Controller;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using uzLib.Lite.Extensions;

namespace HitsSniffer.Model
{
    [DbTableName("hit_counter")]
    public class HitData : IPrimaryKey
    {
        // ===== DATA FROM SNIFFING =====

        public string RawData { get; }

        [DbColumnName("path", 6, MySqlDbType.Text, DbType.String)]
        public string Path { get; set; }

        [DbColumnName("sid", 9, MySqlDbType.Text, DbType.String)]
        public string SID { get; set; }

        // ===== END DATA FROM SNIFFING =====

        // ===== DATA FROM DATABASE =====

        [DbColumnName("id", 1)]
        public int Id { get; set; }

        // TODO
        [DbColumnName("org_owner_id", 2)]
        public int? OrgId { get; set; }

        // TODO
        [DbColumnName("user_owner_id", 3)]
        public int? UserId { get; set; }

        // TODO
        [DbColumnName("repo_id", 4)]
        public int? RepoId { get; set; }

        // ====

        [DbColumnName("date", 5)]
        public DateTime Date { get; set; }

        [DbColumnName("hits", 7)]
        public int Hits { get; set; }

        [DbColumnName("hash", 8, MySqlDbType.Text, DbType.String)]
        public string Hash { get; set; }

        // ===== END DATA FROM DATABASE =====

        private HitData()
        {
        }

        public HitData(string data, string sid)
        {
            RawData = data;
            SID = sid;
        }

        public HitData TransformData()
        {
            // 2019-08-17 17:35:37 /enjoyneering/Arduino_Deep_Sleep 248 X6X96hTtSE

            var parts = RawData.Split(' ');

            Date = DateTime.Parse($"{parts[0]} {parts[1]}");
            Path = parts[2];
            Hits = int.Parse(parts[3]);
            Hash = parts[4];

            SetExternalData();

            return this;
        }

        private void SetExternalData()
        {
            bool toggleFlag = GetDataAsUser(out string userOrOrganization, out string repository);

            var orgId = OrgId;
            var userId = UserId;
            var repoId = RepoId;

            SetIds(userOrOrganization, repository, toggleFlag, out orgId, out userId, out repoId);
        }

        // true = user, false = organization
        private bool GetDataAsUser(out string userOrOrganization, out string repository)
        {
            string path = Path.Substring(1);

            var parts = path.Split('/');

            userOrOrganization = parts[0];
            repository = parts[1];

            return IsUserOrOrg(userOrOrganization);
        }

        private void SetIds(string userOrOrganization, string repository, bool toggleFlag, out int? orgId, out int? userId, out int? repoId)
        {
            // First, we check if the user/org/repo is blacklisted
            bool? isUserBlacklisted = toggleFlag ? !BlacklistWorker.IsValid(userOrOrganization, BlacklistWorker.Type.User) : (bool?)null;
            bool? isOrgBlacklisted = !toggleFlag ? !BlacklistWorker.IsValid(userOrOrganization, BlacklistWorker.Type.Organization) : (bool?)null;
            bool isRepoBlacklisted = !BlacklistWorker.IsValid(repository, BlacklistWorker.Type.Repository);

            // If not, then get ids
            if (toggleFlag)
            {
                userId = isUserBlacklisted == true ? (int?)null :
                    SqlWorker.BeginExistsTransaction<UserData>(userOrOrganization) ?? SqlWorker.RegisterTableValue<UserData>(userOrOrganization);

                // If false, means not registered user/organization/repo in the database, due to not met conditions, then we need to add it to the blacklist
                // If first time, then add to blacklist
                if (!SqlWorker.EndExistsTransaction() && isUserBlacklisted == false && !BlacklistWorker.IsUserListable(userOrOrganization))
                    BlacklistWorker.Add(userOrOrganization, BlacklistWorker.Type.User);

                orgId = null;
            }
            else
            {
                userId = null;

                orgId = isOrgBlacklisted == true ? (int?)null :
                    SqlWorker.BeginExistsTransaction<OrgData>(userOrOrganization) ?? SqlWorker.RegisterTableValue<OrgData>(userOrOrganization);

                // If first time, then add to blacklist
                if (!SqlWorker.EndExistsTransaction() && isOrgBlacklisted == false && !BlacklistWorker.IsOrgListable(userOrOrganization))
                    BlacklistWorker.Add(userOrOrganization, BlacklistWorker.Type.Organization);
            }

            repoId = isRepoBlacklisted ? (int?)null :
                SqlWorker.BeginExistsTransaction<RepoData>(repository) ?? SqlWorker.RegisterTableValue<RepoData>(repository);

            // If first time, then add to blacklist
            if (!SqlWorker.EndExistsTransaction() && !isRepoBlacklisted && !BlacklistWorker.IsRepositoryListable(repository))
                BlacklistWorker.Add(repository, BlacklistWorker.Type.Repository);

            //return (toggleFlag && (!orgId.HasValue || orgId.Value == -1) ||
            //       !toggleFlag && (!userId.HasValue || userId.Value != -1))
            //                   && (!repoId.HasValue || repoId.Value != -1);
        }

        public override string ToString()
        {
            return $"SID: {SID}" +
                   Environment.NewLine +
                   $"Data: {RawData}";
        }

        // true = user, false = organization
        private static bool IsUserOrOrg(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            //const string ContentType = "text/plain; charset=UTF-8";
            //const string UserAgent = "curl/7.65.3";
            //const string AcceptHeader = "*/*";
            const string Selector = "p-nickname";

            string source;

            using (var wc = new WebClient())
                source = wc.DownloadString(string.Format(DriverWorker.TemplateUrl, name));

            var doc = new HtmlDocument();
            doc.LoadHtml(source);

            return doc.DocumentNode.GetNodeByClass(Selector) != null;
        }
    }
}