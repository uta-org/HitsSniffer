using System;

namespace HitsSniffer.Model
{
    public class HitData
    {
        public string Data { get; }
        public string SID { get; }

        private HitData()
        {
        }

        public HitData(string data, string sid)
        {
            Data = data;
            SID = sid;
        }

        public override string ToString()
        {
            return $"SID: {SID}" +
                   Environment.NewLine +
                   $"Data: {Data}";
        }
    }
}