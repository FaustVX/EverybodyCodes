using System.Diagnostics;

namespace EverybodyCodes.Core;

public static class Shell
{
    public static Task StartAsync(string cmd, params ReadOnlySpan<string> args)
    => Process.Start(cmd, [..args])!.WaitForExitAsync();

    public static void Start(string cmd, params ReadOnlySpan<string> args)
    => Process.Start(cmd, [..args])!.WaitForExit();

    public static class Git
    {
        public static Task Commit(string message, params ReadOnlySpan<string> args)
        => StartAsync("git", ["commit", "-m", message, ..args]);
        public static Task Add(params ReadOnlySpan<string> args)
        => StartAsync("git", ["add", ..args]);

        public static class Submodule
        {
            public static Task Add(string repo, string folder, string? branch = null, params ReadOnlySpan<string> args)
            => StartAsync("git", ["submodule", "add", ..branch is not null ? ["-b", branch] : Array.Empty<string>(), "--", repo, folder, ..args]);
        }

        public static class Worktree
        {
            public static Task Add(string? branch, string folder, bool isOrphan = false, params ReadOnlySpan<string> args)
            => StartAsync("git", ["worktree", "add", ..branch is string b ? ["-b", branch] : Array.Empty<string>(), ..isOrphan ? ["--orphan"] : Array.Empty<string>(), folder, ..args]);
        }
    }

    public static class Dotnet
    {
        public static Task New(string template, params ReadOnlySpan<string> args)
        => StartAsync("dotnet", ["new", template, ..args]);

        public static class Sln
        {
            public static Task Add(string project, params ReadOnlySpan<string> args)
            => StartAsync("dotnet", ["sln", "add", project, ..args]);
        }
    }

    public static class VsCode
    {
        public static Task OpenInExistingWindow(params IEnumerable<string> args)
        => Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"), ["--profile", "C#", "--reuse-window", "--", ..args]).WaitForExitAsync();

        public static Task OpenInNewWindow(params IEnumerable<string> args)
        => Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"), ["--profile", "C#", "--new-window", "--", ..args]).WaitForExitAsync();
    }

}
