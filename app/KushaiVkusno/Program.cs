using KushaiVkusno.Forms;
using KushaiVkusno.Tools;

namespace KushaiVkusno;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length > 0 && string.Equals(args[0], "--selftest", StringComparison.OrdinalIgnoreCase))
        {
            var report = args.Length > 1
                ? args[1]
                : Path.Combine(AppContext.BaseDirectory, "selftest.txt");
            SelfTest.Run(report);
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "--screenshots", StringComparison.OrdinalIgnoreCase))
        {
            var target = args.Length > 1
                ? args[1]
                : Path.Combine(AppContext.BaseDirectory, "screenshots");
            ScreenshotRunner.Run(target);
            return;
        }

        Application.Run(new LoginForm());
    }
}
