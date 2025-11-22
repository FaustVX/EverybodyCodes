using System.Diagnostics;

namespace EverybodyCodes.Core;

public static class Shell
{
    public static Task StartAsync(string cmd, params ReadOnlySpan<string> args)
    => Process.Start(cmd, [..args])!.WaitForExitAsync();

    public static Task<string> StartOutputAsync(string cmd, params ReadOnlySpan<string> args)
    {
        var psi = new ProcessStartInfo(cmd, [.. args])
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };
        return Execute(psi);

        static async Task<string> Execute(ProcessStartInfo psi)
        {
            using var process = new Process()
            {
                StartInfo = psi,
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();
            return output;
        }
    }

    private static string[] Expand(bool b, string @true = "", string @false = "")
    => (b ? @true : @false) is var s && !string.IsNullOrWhiteSpace(s) ? [s] : Array.Empty<string>();

    private static string[] Expand(bool? b, string @true = "", string @false = "", string @null = "")
    => (b switch
    {
        true => @true,
        false => @false,
        null => @null,
    }) is var s && !string.IsNullOrWhiteSpace(s) ? [s] : Array.Empty<string>();

    public static class Git
    {
        public static Task Commit(string message, bool quiet = true, params ReadOnlySpan<string> args)
        => StartAsync("git", ["commit", ..Expand(quiet, "--quiet"), "-m", message, ..args]);
        public static Task Add(params ReadOnlySpan<string> args)
        => StartAsync("git", ["add", ..args]);
        public static Task Init(params ReadOnlySpan<string> args)
        => StartAsync("git", ["init", ..args]);

        public static class Submodule
        {
            public static Task Add(string repo, string folder, string? branch = null, params ReadOnlySpan<string> args)
            => StartAsync("git", ["submodule", "add", ..branch is not null ? ["-b", branch] : Array.Empty<string>(), ..args, "--", repo, folder]);
            public static Task Update(bool init = false, bool recursive = false, params ReadOnlySpan<string> args)
            => StartAsync("git", ["submodule", "update", ..Expand(init, "--init"), ..Expand(recursive, "--recursive"), ..args]);
        }

        public static class Worktree
        {
            public static Task Add(string? branch, string folder, bool orphan = false, params ReadOnlySpan<string> args)
            => StartAsync("git", ["worktree", "add", ..branch is string b ? ["-b", branch] : Array.Empty<string>(), ..Expand(orphan, "--orphan"), folder, ..args]);
        }
        public static Task Checkout(string branch, bool create = false, bool quiet = true, params ReadOnlySpan<string> args)
        => StartAsync("git", ["checkout", ..Expand(quiet, "--quiet"), ..args, ..Expand(create, "-b"), branch]);

        public static Task Merge(ReadOnlySpan<string> branches, bool? ff = null, bool quiet = true, params ReadOnlySpan<string> args)
        => StartAsync("git", ["merge", ..Expand(quiet, "--quiet"), ..args, ..Expand(ff, "--ff", "--no-ff"), ..branches]);

        public static Task Merge(string branch, bool? ff = null, bool quiet = true, params ReadOnlySpan<string> args)
        => Merge([branch], ff, quiet, args);

        public static Task<string> RevParse(string @ref, bool abbrevRef = false)
        => StartOutputAsync("git", ["rev-parse", ..Expand(abbrevRef, "--abbrev-ref"), @ref]);
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
        private static readonly string code = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe");
        /// <summary>
        /// Set to <see langword="null"/> if you don't use profile in VScode
        /// </summary>
        private static readonly string? profile = "C#";

        public static Task OpenInExistingWindow(IEnumerable<string> files, bool wait = false, params IEnumerable<string> args)
        => StartAsync(code, [..profile is not null ? ["--profile", profile] : Array.Empty<string>(), "--reuse-window", ..Expand(wait, "--wait"), ..args, "--", ..files]);

        public static Task OpenInNewWindow(IEnumerable<string> files, bool wait = false, params IEnumerable<string> args)
        => StartAsync(code, [..profile is not null ? ["--profile", profile] : Array.Empty<string>(), "--new-window", ..Expand(wait, "--wait"), ..args, "--", ..files]);
    }
}
