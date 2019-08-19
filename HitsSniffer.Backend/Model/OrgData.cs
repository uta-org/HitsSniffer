using System;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;

namespace HitsSniffer.Model
{
    [DbTableName("organizations", true)]
    [DbTableName("organization_stats")]
    public class OrgData : IData
    {
        [DbColumnName("id")]
        public int Id { get; set; }

        [DbColumnName("name")]
        public string Name { get; set; }

        [DbColumnName("date")]
        public DateTime Date { get; set; }

        [DbColumnName("members")]
        public int Members { get; set; }

        [DbColumnName("repositories")]
        public int Repositories { get; set; }

        [DbColumnName("packages")]
        public int Packages { get; set; }

        [DbColumnName("teams")]
        public int Teams { get; set; }

        [DbColumnName("projects")]
        public int Projects { get; set; }
    }
}