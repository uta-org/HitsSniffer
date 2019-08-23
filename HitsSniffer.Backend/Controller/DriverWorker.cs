using System;
using System.Net;
using HitsSniffer.Model;
using OpenQA.Selenium.Chrome;

namespace HitsSniffer.Controller
{
    public static class DriverWorker
    {
        public static ChromeDriver Driver { get; private set; }
        public const string TemplateUrl = "https://github.com/{0}";

        public static void PrepareDriver()
        {
            if (Driver == null)
            {
                var options = new ChromeOptions();
                options.AddArgument("headless");
                options.AddArgument("disable-gpu");

                Driver = new ChromeDriver(options);
            }
        }

        public static void CloseDriver()
        {
            if (Driver == null)
                return;

            Driver.Close();
            Driver.Dispose();
        }

        public static void PatchWithStatusCode(RepoData data)
        {
            string url = string.Format(TemplateUrl, data);

            int statusNumber;
            string location;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    // This will have statii from 200 to 30x
                    statusNumber = (int)response.StatusCode;
                    location = response.Headers["Location"];
                }
            }
            catch
            // (WebException we)
            {
                // Statii 400 to 50x will be here
                //statusNumber = (int)((HttpWebResponse)we.Response).StatusCode;
                return;
            }

            string oldOwner = data.OwnerName;
            if (statusNumber >= 300 && statusNumber < 400)
            {
                var uri = new Uri(location);
                if (!uri.Host.ToLowerInvariant().Contains("github.com"))
                    throw new InvalidOperationException();

                var parts = location.Split('/');

                string newOwner = parts[4];
                data.OwnerName = newOwner; // TODO: Press F10 and debug newOwner applied to this RepoData instance and the oldOne
            }
        }
    }
}