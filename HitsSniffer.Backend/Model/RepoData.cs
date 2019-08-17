using System;
using HitsSniffer.Model.Interfaces;

namespace HitsSniffer.Model
{
    public class RepoData : IData
    {
        public int Id { get; set; }
        public int? OrgId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Commits { get; set; }
        public int Branches { get; set; }
        public int Releases { get; set; }
        public int Contributors { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public int Watchers { get; set; }
        public int Pulls { get; set; }
        public int Projects { get; set; }
        public int Hits { get; set; }
        public DateTime LastCommit { get; set; }
    }
}