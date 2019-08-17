using HitsSniffer.Controller;
using Console = Colorful.Console;

namespace HitsSniffer
{
    // TODO: Create a blacklist worker, where users/repos/orgs without enough activity will not be tracked
    // Conditions must be: activity in the last 10 days, at least a week (repos/orgs) and one month (users) old, at least 50 commits (users/repos) and 5 repos (orgs)
    // TODO: Search how to awake this process when a reboot is done in Linux

    internal sealed class Program
    {
        private static IntervalHitsWorker HitsWorker { get; } = IntervalHitsWorker.Instance;

        private static void Main(string[] args)
        {
            HitsWorker.StartWorking();
            Console.ReadKey(true);
        }
    }
}