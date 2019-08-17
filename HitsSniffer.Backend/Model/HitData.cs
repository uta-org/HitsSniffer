using System;
using System.Data;
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

        [DbColumnName("path", 6, MySqlDbType.Text, DbType.String)]
        public string Path { get; set; }

        [DbColumnName("sid", 9, MySqlDbType.Text, DbType.String)]
        public string SID { get; }

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

            return this;
        }

        public override string ToString()
        {
            return $"SID: {SID}" +
                   Environment.NewLine +
                   $"Data: {RawData}";
        }
    }
}