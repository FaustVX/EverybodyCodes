using System.Reflection;
using Cocona;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Attributes;
using Newtonsoft.Json.Linq;

var app = CoconaLiteApp.Create(args);

app.AddCommand("run", async ([Argument] int year, [Argument] int day, [Argument] int? part, [Option('s')] string session) =>
{
    Globals.IsTest = false;
    var me = await Me.CreateAsync(session);
    var input = await me.GetPartsAsync(year, day);
    var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
    var solution = (ISolution)Activator.CreateInstance(type!)!;
    if (part is int p)
    {
        var addFinalLF = type!.GetMethod($"Solve{p}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
        var input1 = addFinalLF ? input[p - 1].Input + "\n" : input[p - 1].Input;
        var startTime = TimeProvider.System.GetTimestamp();
        var output = solution.Solve(p, input1);
        Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
        if (input[p - 1].Answers.Length > p - 1)
            Console.WriteLine($"Y{year}D{day:00}P{p} : {output} ({input[p - 1].Answers[p - 1]})");
        else
            Console.WriteLine($"Y{year}D{day:00}P{p} : {output}");
        var response = await input[p - 1].AnswerAsync(output);
        Console.WriteLine(response);
        if (response.IsCorrect && response.Time != default)
        {
            await Shell.Git.Add($"D{day:00}/");
            await Shell.Git.Commit($"D{day:00}/{p}");
        }
    }
    else
        for (p = 1; p <= input.Length; p++)
        {
            var addFinalLF = type!.GetMethod($"Solve{p}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
            var input1 = addFinalLF ? input[p - 1].Input + "\n" : input[p - 1].Input;
            var startTime = TimeProvider.System.GetTimestamp();
            var output = solution.Solve(p, input1);
            Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
            if (input[p - 1].Answers.Length > p - 1)
                Console.WriteLine($"Y{year}D{day:00}P{p} : {output} ({input[p - 1].Answers[p - 1]})");
            else
                Console.WriteLine($"Y{year}D{day:00}P{p} : {output}");
            var response = await input[p - 1].AnswerAsync(output);
            Console.WriteLine(response);
            if (response.IsCorrect && response.Time != default)
            {
                await Shell.Git.Add($"D{day:00}/");
                await Shell.Git.Commit($"D{day:00}/{p}");
            }
        }
});

app.AddCommand("get", async ([Argument] int year, [Argument] int day, [Option('s')] string session) =>
{
    var me = await Me.CreateAsync(session);
    var input = await me.GetInputAsync(year, day);
    await Shell.Git.Add($"D{day:00}/");
    await Shell.Git.Commit($"D{day:00}");
    await Shell.VsCode.OpenInExistingWindow($"D{day:00}/Solution.cs", $"D{day:00}/input.json", $"D{day:00}/test1.json");
});

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
        var addFinalLF = type!.GetMethod($"Solve{p}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
        var input1 = addFinalLF ? input!.Input + "\n" : input!.Input;
        var startTime = TimeProvider.System.GetTimestamp();
        var output = solution.Solve(p, input1);
        Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
        Console.WriteLine($"Y{year}D{day:00}P{p} : {output} ({input.Answers[p - 1]})");
    }
    else
    {
        var input = Me.GetTestParts(year, day, file);
        var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
        var solution = (ISolution)Activator.CreateInstance(type!)!;
        for (p = 1; p <= input.Length; p++)
        {
            if (input[p - 1] is not {} a)
                continue;
            var addFinalLF = type!.GetMethod($"Solve{p}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
            var input1 = addFinalLF ? a.Input + "\n" : a.Input;
            var startTime = TimeProvider.System.GetTimestamp();
            var output = solution.Solve(p, input1);
            Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
            Console.WriteLine($"Y{year}D{day:00}P{p} : {output} ({a.Answers[p - 1]})");
        }
    }
});

app.AddCommand("new", async ([Argument] int year, [Option('r')] string repo = "git@github.com:FaustVX/EverybodyCodes.git", [Option('b')] string branch = "main") =>
{
#pragma warning disable CS9193 // Argument should be a variable because it is passed to a 'ref readonly' parameter
    var worktree = Path.Combine(["..", ..!Directory.Exists("lib") ? new ReadOnlySpan<string>("..") : [], year.ToString()]);
#pragma warning restore CS9193 // Argument should be a variable because it is passed to a 'ref readonly' parameter
    await Shell.Git.Worktree.Add($"years/{year}", worktree, isOrphan: true);
    Environment.CurrentDirectory = worktree;
    var vscode = Directory.CreateDirectory(".vscode");
    await Shell.Git.Submodule.Add(repo, "lib", branch);
    var csproj = new FileInfo(Path.Combine("lib", "EverybodyCodes.Core", "EverybodyCodes.Core.csproj"));
    var launch = new FileInfo(Path.Combine("lib", ".vscode", "launch.json"));
    var tasks = new FileInfo(Path.Combine("lib", ".vscode", "tasks.json"));
    var extensions = new FileInfo(Path.Combine("lib", ".vscode", "extensions.json"));
    var settings = new FileInfo(Path.Combine("lib", ".vscode", "settings.json"));

    var text = csproj.ReadToEnd().Replace("'false'", "'true'");
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
}
