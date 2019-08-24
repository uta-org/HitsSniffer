using System;
using System.Data;
using HitsSniffer.Controller;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;
using MySql.Data.MySqlClient;

namespace HitsSniffer.Model
{
    [DbTableName("hit_counter")]
    public class HitData : IPrimaryKey
    {
        // ===== DATA FROM SNIFFING =====

        public string RawData { get; }

        [DbColumnName("path", 4, MySqlDbType.Text, DbType.String)]
        public string Path { get; set; }

        [DbColumnName("sid", 7, MySqlDbType.Text, DbType.String)]
        public string SID { get; set; }

        // ===== END DATA FROM SNIFFING =====

        // ===== DATA FROM DATABASE =====

        [DbColumnName("id", 1)]
        public int Id { get; set; }

        //// TODO
        //[DbColumnName("org_owner_id", 2)]
        //public int? OrgId { get; set; }

        //// TODO
        //[DbColumnName("user_owner_id", 3)]
        //public int? UserId { get; set; }

        // TODO
        [DbColumnName("repo_id", 2)]
        public int? RepoId { get; set; }

        // ====

        [DbColumnName("date", 3)]
        public DateTime Date { get; set; }

        [DbColumnName("hits", 5)]
        public int Hits { get; set; }

        [DbColumnName("hash", 6, MySqlDbType.Text, DbType.String)]
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
            var repository = GetRepo();
            SetIds(repository, out var repoId);

            RepoId = repoId;
        }

        // true = user, false = organization
        private RepoData GetRepo()
        {
            RepoData data;

            if (Path.Contains("http"))
            {
                string path = Path.Substring(1);
                var parts = path.Split('/');

                data = new RepoData(parts[4], parts[5]);
            }
            else
            {
                string path = Path.Substring(1);
                var parts = path.Split('/');

                data = new RepoData(parts[1], parts[0]);
            }

            DriverWorker.PatchWithStatusCode(data);

            return data;
        }

        private void SetIds(RepoData repository, out int? repoId)
        {
            // TODO: I need that line 107 executes before whitelist string array set
            bool isRepoBlacklisted = !BlacklistWorker.IsValid(repository.ToString(), BlacklistWorker.Type.Repository);

            repoId = isRepoBlacklisted ? (int?)null :
                SqlWorker.BeginExistsTransaction<RepoData>(repository.Name) ?? repository.RegisterTableValue(repository.GetTypeFromOwner());

            // If first time, then add to blacklist
            if (!SqlWorker.EndExistsTransaction() && !isRepoBlacklisted && !BlacklistWorker.IsRepositoryListable(repository.ToString()))
                BlacklistWorker.Add(repository.ToString(), BlacklistWorker.Type.Repository);
        }

        public override string ToString()
        {
            return $"SID: {SID}" +
                   Environment.NewLine +
                   $"Data: {RawData}";
        }

        public string Identifier()
        {
            return "Hit";
        }
    }
}