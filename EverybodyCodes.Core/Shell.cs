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
        public static Task Init(params ReadOnlySpan<string> args)
        => StartAsync("git", ["init", ..args]);

        public static class Submodule
        {
            public static Task Add(string repo, string folder, string? branch = null, params ReadOnlySpan<string> args)
            => StartAsync("git", ["submodule", "add", ..branch is not null ? ["-b", branch] : Array.Empty<string>(), ..args, "--", repo, folder]);
            public static Task Update(bool init = false, bool recursive = false, params ReadOnlySpan<string> args)
            => StartAsync("git", ["submodule", "update", ..init ? ["--init"] : Array.Empty<string>(), ..recursive ? ["--recursive"] : Array.Empty<string>(), ..args]);
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
        public static Task OpenInExistingWindow(IEnumerable<string> files, bool wait = false, params IEnumerable<string> args)
        => StartAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"), ["--profile", "C#", "--reuse-window", ..wait ? ["--wait"] : Array.Empty<string>(), ..args, "--", ..files]);

        public static Task OpenInNewWindow(IEnumerable<string> files, bool wait = false, params IEnumerable<string> args)
        => StartAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"), ["--profile", "C#", "--new-window", ..wait ? ["--wait"] : Array.Empty<string>(), ..args, "--", ..files]);
    }
}
