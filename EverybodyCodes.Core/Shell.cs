using System.Diagnostics;

namespace EverybodyCodes.Core;

public static class Shell
{
    public static Task StartAsync(string cmd, params IEnumerable<string> args)
    => Process.Start(cmd, args)!.WaitForExitAsync();

    public static void Start(string cmd, params IEnumerable<string> args)
    => Process.Start(cmd, args)!.WaitForExit();

    public static class Git
    {
        public static Task Commit(string message, params IEnumerable<string> args)
        => StartAsync("git", ["commit", "-m", message, ..args]);
        public static Task Add(params IEnumerable<string> args)
        => StartAsync("git", ["add", ..args]);
    }

    public static Task OpenVsCode(params IEnumerable<string> args)
    => Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"), ["--profile", "C#", "--reuse-window", "--", ..args]).WaitForExitAsync();
}
