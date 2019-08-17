using System;
using HitsSniffer.Model.Interfaces;

namespace HitsSniffer.Model
{
    public class OrgData : IData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Members { get; set; }
        public int Repositories { get; set; }
        public int Packages { get; set; }
        public int Teams { get; set; }
        public int Projects { get; set; }
    }
}