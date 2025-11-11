using System.Globalization;
using System.Numerics;
using System.Reflection;
using Cocona;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Attributes;
using Newtonsoft.Json.Linq;
using Spectre.Console;

var app = CoconaLiteApp.Create(args);

app.AddCommand("run", async ([Argument] int year, [Argument] int day, [Argument] int? part, [Option('s')] string session)
=> await AnsiConsole.Progress()
    .Columns([
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new ElapsedTimeColumn() { Format = null },
    ])
    .StartAsync(async ctx =>
    {
        var getMeTask = ctx.AddTask("Getting Infos", maxValue: 1, autoStart: true);
        getMeTask.IsIndeterminate = true;
        var getInputTask = ctx.AddTask("Getting input", maxValue: 1, autoStart: false);
        getInputTask.IsIndeterminate = true;
        var instanciateTask = ctx.AddTask("Create Solution", maxValue: 2, autoStart: false);
        instanciateTask.IsIndeterminate = true;

        try
        {
            Globals.IsTest = false;
            var me = await Me.CreateAsync(session);
            getMeTask.NextTask(ctx, getInputTask);
            var input = await me.GetPartsAsync(year, day);
            getInputTask.NextTask(ctx, instanciateTask);
            var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
            var solution = (ISolution)Activator.CreateInstance(type!)!;
            if (part is int p)
                await RunPart(year, day, input, type!, solution, p, ctx, instanciateTask, session);
            else
                for (p = 1; p <= input.Length; p++)
                    await RunPart(year, day, input, type!, solution, p, ctx, instanciateTask, session);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return;
        }

        static async Task RunPart(int year, int day, Part[] input, Type type, ISolution solution, int p, ProgressContext ctx, ProgressTask instanciateTask, string session)
        {
            instanciateTask.Next(ctx);
            var addFinalLF = type.GetMethod($"Solve{p}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
            var input1 = addFinalLF ? input[p - 1].Input + "\n" : input[p - 1].Input;
            var solvingTask = ctx.AddTask("Solving", maxValue: 1, autoStart: false);
            instanciateTask.NextTask(ctx, solvingTask);
            var output = solution.Solve(p, input1);
            var sendingTask = ctx.AddTask("Sending answer", autoStart: false, maxValue: 1);
            sendingTask.Description = input[p - 1].Answers.Length > p - 1
                ? PrintResult(year, day, p, output, input[p - 1].Answers[p - 1])
                : PrintResult(year, day, p, output, null);
            solvingTask.NextTask(ctx, sendingTask);
            var response = await input[p - 1].AnswerAsync(output);
            // Console.WriteLine(response);
            if (response.IsCorrect && response.Time != default)
            {
                var gitTask = ctx.AddTask("Git commit", maxValue: 1, autoStart: false);
                sendingTask.NextTask(ctx, gitTask);
                await Shell.Git.Add($"D{day:00}/");
                await Shell.Git.Commit($"D{day:00}/{p}");
            }
            else if (!response.IsCorrect)
            {
                var me = await Me.CreateAsync(session);
                var waitTask = ctx.AddTask("Wait 60 seconds", maxValue: me.PenaltySpan.TotalSeconds, autoStart: false);
                sendingTask.NextTask(ctx, waitTask);
                while (!ctx.IsFinished)
                {
                    if (me.PenaltySpan.TotalSeconds >= 10)
                        await Task.Delay(TimeSpan.FromSeconds(.5));
                    me = await Me.CreateAsync(session);
                    waitTask.Value = waitTask.MaxValue - me.PenaltySpan.TotalSeconds;
                    waitTask.Description = $"Wait {(int)me.PenaltySpan.TotalSeconds} seconds";
                }
            }
            else
                sendingTask.Next(ctx);
        }
    }));

app.AddCommand("get", async ([Argument] int year, [Argument] int day, [Option('s')] string session)
=> await AnsiConsole.Progress()
    .Columns(
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new ElapsedTimeColumn() { Format = null }
    )
    .StartAsync(async ctx =>
    {
        var getInfoTask = ctx.AddTask("Getting Infos", maxValue: 1, autoStart: true);
        getInfoTask.IsIndeterminate = true;
        var getInputTask = ctx.AddTask("Getting input", maxValue: 1, autoStart: false);
        getInputTask.IsIndeterminate = true;

        try
        {
            var me = await Me.CreateAsync(session);
            getInfoTask.NextTask(ctx, getInputTask);
            var input = await me.GetInputAsync(year, day);
            // var getDescriptionTask = ctx.AddTask("Getting description", maxValue: 1, autoStart: false);
            // getDescriptionTask.IsIndeterminate = true;
            // getInputTask.NextTask(ctx, getDescriptionTask);
            // var description = await me.GetDescriptionAsync(year, day);
            var gitTask = ctx.AddTask("Git commit", maxValue: 3, autoStart: false);
            getInputTask.NextTask(ctx, gitTask);
            await Shell.Git.Add($"D{day:00}/");
            gitTask.Next(ctx);
            await Shell.Git.Commit($"D{day:00}");
            gitTask.Next(ctx);
            await Shell.VsCode.OpenInExistingWindow($"D{day:00}/Solution.cs", $"D{day:00}/input.json", $"D{day:00}/test1.json");
            gitTask.Next(ctx);
        }
        catch (Exception ex)
        {
            ctx.Refresh();
            AnsiConsole.WriteException(ex);
            if (ex is OutOfDateException { DurationRemaining.TotalSeconds: var sec, AvailableFrom: var from })
            {
                var waitTask = ctx.AddTask("Wait x minutes", maxValue: sec, autoStart: false);
                getInputTask.NextTask(ctx, waitTask);
                while (!ctx.IsFinished)
                {
                    var duration = from - TimeProvider.System.GetUtcNow();
                    waitTask.Value = waitTask.MaxValue - duration.TotalSeconds;
                    waitTask.Description = duration switch
                    {
                        { TotalSeconds: < 100 } => $"Wait {(int)duration.TotalSeconds} seconds",
                        { TotalMinutes: < 100 } => $"Wait {(int)duration.TotalMinutes} minutes",
                        _ => $"Wait {(int)duration.TotalHours} hours",
                    };
                    await Task.Delay(TimeSpan.FromSeconds(.5));
                }
            }
            return;
        }
    }));

app.AddCommand("test", ([Argument] int year, [Argument] int day, [Argument] int? part, [Option('f')]string file) =>
{
    Globals.IsTest = true;
    Console.WriteLine("Test file: " + Path.GetRelativePath(Path.GetFullPath($"D{day:00}"), file));
    if (part is int p)
    {
        if (Me.GetTestPart(year, day, p, file) is not {} input)
        {
            Console.WriteLine($"The part {p} in test file is null");
            return;
        }

        var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
        var solution = (ISolution)Activator.CreateInstance(type!)!;
        RunPart(year, day, p, type!, solution, input);
    }
    else
    {
        var input = Me.GetTestParts(year, day, file);
        var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
        var solution = (ISolution)Activator.CreateInstance(type!)!;
        for (p = 1; p <= input.Length; p++)
            if (input[p - 1] is { } a)
                RunPart(year, day, p, type!, solution, a);
    }

    static void RunPart(int year, int day, int p, Type type, ISolution solution, Part a)
    {
        var addFinalLF = type!.GetMethod($"Solve{p}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
        var input1 = addFinalLF ? a.Input + "\n" : a.Input;
        var startTime = TimeProvider.System.GetTimestamp();
        var output = solution.Solve(p, input1);
        Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
        AnsiConsole.MarkupLine(PrintResult(year, day, p, output, a.Answers[p - 1]));
    }
});

app.AddCommand("new", async ([Argument] int year, [Option('f')]string? folder, [Option('r')] string repo = "git@github.com:FaustVX/EverybodyCodes.git", [Option('b')] string branch = "main", [Option('w')] bool withoutWorktree = false) =>
{
    var worktree = folder ?? Path.Combine(["..", ..!Directory.Exists("lib") ? [".."] : ReadOnlySpan<string>.Empty, year.ToString()]);
    if (withoutWorktree)
    {
        Directory.CreateDirectory(worktree);
        Environment.CurrentDirectory = worktree;
        await Shell.Git.Init();
    }
    else
    {
        await Shell.Git.Worktree.Add($"years/{year}", worktree, isOrphan: true);
        Environment.CurrentDirectory = worktree;
    }

    var vscode = Directory.CreateDirectory(".vscode");
    await Shell.Git.Submodule.Add(repo, "lib", branch);
    await Shell.Git.Submodule.Update(init: true, recursive: true);
    var csproj = new FileInfo(Path.Combine("lib", "EverybodyCodes.Core", "EverybodyCodes.Core.csproj"));
    var launch = new FileInfo(Path.Combine("lib", ".vscode", "launch.json"));
    var tasks = new FileInfo(Path.Combine("lib", ".vscode", "tasks.json"));
    var extensions = new FileInfo(Path.Combine("lib", ".vscode", "extensions.json"));
    var settings = new FileInfo(Path.Combine("lib", ".vscode", "settings.json"));

    var text = csproj.ReadToEnd()
        .Replace("""Include="../spectre""", """Include="lib/spectre""")
        .Replace("'false'", "'true'");
    File.WriteAllText("EverybodyCodes.csproj", text);
    await Shell.Dotnet.New("gitignore");
    await Shell.Dotnet.New("sln");
    await Shell.Dotnet.Sln.Add("EverybodyCodes.csproj");

    text = launch.ReadToEnd()
        .Replace("/EverybodyCodes.Core.dll", "/EverybodyCodes.dll")
        .Replace("/EverybodyCodes.Core", "")
        .Replace("${input:year}", year.ToString());
    var json = JObject.Parse(text);
    if (year < 2000) // Stories
    {
        var days = (JArray)json["inputs"]![1]!["args"]!["options"]!;
        for (var i = days.Count - 1; i >= 3; i--)
            days[i].Remove();
    }
    ((JArray)json["inputs"]!)[0].Remove();
    File.WriteAllText(Path.Combine(vscode.FullName, launch.Name), json.ToString());

    text = tasks.ReadToEnd()
        .Replace("/EverybodyCodes.Core", "/EverybodyCodes.csproj")
        .Replace("${input:year}", year.ToString());
    json = JObject.Parse(text);
    if (year < 2000) // Stories
    {
        var days = (JArray)json["inputs"]![1]!["args"]!["options"]!;
        for (var i = days.Count - 1; i >= 3; i--)
            days[i].Remove();
    }
    ((JArray)json["inputs"]!)[0].Remove();
    File.WriteAllText(Path.Combine(vscode.FullName, tasks.Name), json.ToString());
    File.WriteAllText(Path.Combine(vscode.FullName, extensions.Name), extensions.ReadToEnd());
    File.WriteAllText(Path.Combine(vscode.FullName, settings.Name), settings.ReadToEnd());

    await Shell.Git.Add(".");
    await Shell.Git.Commit(year.ToString());
    await Shell.VsCode.OpenInNewWindow("./");
});

app.Run();

static string PrintResult(int year, int day, int part, string answer, string? expected)
{
    var output = $"Y{year}D{day:00}P{part} : ";
    if (expected is not null)
        output += answer == expected ? "[green]" : "[red]";
    output += answer;
    output += "[/]";
    if (expected is not null)
    {
        var op = Operator<long>(answer, expected)
            ?? Operator<decimal>(answer, expected)
            ?? (answer == expected ? "==" : "!=");
        output += $" {op} {expected}";
    }

    return output;

    static string? Operator<T>(string answer, string expected)
    where T : IParsable<T>, IComparisonOperators<T, T, bool>
    {
        if (T.TryParse(answer, CultureInfo.InvariantCulture, out var a) && T.TryParse(expected, CultureInfo.InvariantCulture, out var e))
            if (a < e)
                return "<";
            else if (a > e)
                return ">";
            else
                return "==";
        return null;
    }
}

file static class Ext
{
    extension(FileInfo file)
    {
        public string ReadToEnd()
        {
            using var fs = file.OpenText();
            return fs.ReadToEnd();
        }
    }

    extension(ProgressTask task)
    {
        public void NextTask(ProgressContext ctx, ProgressTask next, bool isDetermined = false)
        {
            task.Value = task.MaxValue;
            task.StopTask();
            if (isDetermined)
                next.IsIndeterminate = false;
            ctx.Refresh();
            next.StartTask();
        }
        public void Next(ProgressContext ctx)
        {
            task.Increment(1);
            ctx.Refresh();
        }
    }
}
