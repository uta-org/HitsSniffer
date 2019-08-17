using System;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;

namespace HitsSniffer.Model
{
    [DbTableName("repository_stats")]
    public class RepoData : IData
    {
        [DbColumnName("id")]
        public int Id { get; set; }

        [DbColumnName("org_owner_id")]
        public int? OrgId { get; set; }

        [DbColumnName("user_owner_id")]
        public int? UserId { get; set; }

        [DbColumnName("name")]
        public string Name { get; set; }

        [DbColumnName("date")]
        public DateTime Date { get; set; }

        [DbColumnName("commits")]
        public int Commits { get; set; }

        [DbColumnName("branches")]
        public int Branches { get; set; }

        [DbColumnName("releases")]
        public int Releases { get; set; }

        [DbColumnName("contributors")]
        public int Contributors { get; set; }

        [DbColumnName("stars")]
        public int Stars { get; set; }

        [DbColumnName("forks")]
        public int Forks { get; set; }

        [DbColumnName("watchers")]
        public int Watchers { get; set; }

        [DbColumnName("pulls")]
        public int Pulls { get; set; }

        [DbColumnName("projects")]
        public int Projects { get; set; }

        [DbColumnName("hits")]
        public int Hits { get; set; }

        [DbColumnName("last_commit")]
        public DateTime LastCommit { get; set; }
    }
}