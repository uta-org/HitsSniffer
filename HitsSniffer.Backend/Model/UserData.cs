using System;
using HitsSniffer.Model.Interfaces;

namespace HitsSniffer.Model
{
    public class UserData : IData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public int Repositories { get; set; }
        public int Commits { get; set; }
        public int Projects { get; set; }
        public int StarsGiven { get; set; }
    }
}