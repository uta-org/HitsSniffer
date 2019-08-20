using System;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;

namespace HitsSniffer.Model
{
    [DbTableName("users", true)]
    [DbTableName("user_stats")]
    public class UserData : IData
    {
        [DbColumnName("id")]
        public int Id { get; set; }

        [DbColumnName("name")]
        public string Name { get; set; }

        [DbColumnName("date")]
        public DateTime Date { get; set; }

        [DbColumnName("followers")]
        public int Followers { get; set; }

        [DbColumnName("following")]
        public int Following { get; set; }

        [DbColumnName("repositories")]
        public int Repositories { get; set; }

        [DbColumnName("commits")]
        public int Commits { get; set; }

        [DbColumnName("commits_last_year")]
        public int CommitsLastYear { get; set; }

        [DbColumnName("projects")]
        public int Projects { get; set; }

        [DbColumnName("stars_given")]
        public int StarsGiven { get; set; }
    }
}