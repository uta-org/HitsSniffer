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
    }
}