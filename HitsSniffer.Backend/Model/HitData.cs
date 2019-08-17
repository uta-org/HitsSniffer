using System;

namespace HitsSniffer.Model
{
    public class HitData
    {
        // ===== DATA FROM SNIFFING =====

        public string RawData { get; } // AKA Path in DB

        public string SID { get; }

        // ===== END DATA FROM SNIFFING =====

        // ===== DATA FROM DATABASE =====

        public int Id { get; set; }
        public int OrgId { get; set; }
        public int UserId { get; set; }

        public DateTime Date { get; set; }
        public int Hits { get; set; }
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

        public void TransformData()
        {
        }

        public override string ToString()
        {
            return $"SID: {SID}" +
                   Environment.NewLine +
                   $"Data: {RawData}";
        }
    }
}