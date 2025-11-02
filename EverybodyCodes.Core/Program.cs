using System.Reflection;
using Cocona;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Attributes;
using Newtonsoft.Json.Linq;

var app = CoconaLiteApp.Create(args);

app.AddCommand("run", async ([Argument] int year, [Argument] int day, [Argument] int part, [Option('s')] string session) =>
{
    var me = await Me.CreateAsync(session);
    var input = await me.GetInputAsync(year, day, part);
    var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
    var solution = (ISolution)Activator.CreateInstance(type!)!;
    var addFinalLF = type!.GetMethod($"Solve{part}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
    var input1 = addFinalLF ? input.Input + "\n" : input.Input;
    var startTime = TimeProvider.System.GetTimestamp();
    var output = solution.Solve(part, input1);
    Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
    Console.WriteLine($"Y{year}D{day:00}P{part} : {output}");
    var response = await input.AnswerAsync(output);
    Console.WriteLine(response);
    if (response.IsCorrect && response.Time != default)
    {
        await Shell.Git.Add($"D{day:00}/");
        await Shell.Git.Commit($"D{day:00}/{part}");
    }
    return response switch
    {
        { IsCorrect: true } => 0,
        { IsLengthCorrect: true, IsFirstCorrect: true } => 1,
        { IsFirstCorrect: true } => 2,
        { IsLengthCorrect: true } => 3,
        _ => -1,
    };
});

app.AddCommand("get", async ([Argument] int year, [Argument] int day, [Option('s')] string session) =>
{
    var me = await Me.CreateAsync(session);
    var input = await me.GetInputAsync(year, day);
    await Shell.Git.Add($"D{day:00}/");
    await Shell.Git.Commit($"D{day:00}");
    await Shell.VsCode.OpenInExistingWindow($"D{day:00}/Solution.cs", $"D{day:00}/input.json");
});

app.AddCommand("test", ([Argument] int year, [Argument] int day, [Argument]string file, [Argument] int? part) =>
{
    if (part is int p)
    {
        var input = Me.GetTestInputAsync(year, day, p, file);
        var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
        var solution = (ISolution)Activator.CreateInstance(type!)!;
        var addFinalLF = type!.GetMethod($"Solve{p}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
        var input1 = addFinalLF ? input.Input + "\n" : input.Input;
        var startTime = TimeProvider.System.GetTimestamp();
        var output = solution.Solve(p, input1);
        Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
        Console.WriteLine($"Y{year}D{day:00}P{p} : {output}");
    }
    else
    {
        var input = Me.GetTestInputAsync(year, day, file);
        var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
        var solution = (ISolution)Activator.CreateInstance(type!)!;
        for (var i = 0; i < input.Length; i++)
        {
            var addFinalLF = type!.GetMethod($"Solve{i + 1}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
            var input1 = addFinalLF ? input[i].Input + "\n" : input[i].Input;
            var startTime = TimeProvider.System.GetTimestamp();
            var output = solution.Solve(i + 1, input1);
            Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
            Console.WriteLine($"Y{year}D{day:00}P{i + 1} : {output}");
        }
    }
});

app.AddCommand("new", async ([Argument] int year, [Option('r')] string repo = "git@github.com:FaustVX/EverybodyCodes.git") =>
{
    var worktree = $"../{year}";
    await Shell.Git.Worktree.Add($"years/{year}", worktree, isOrphan: true);
    Environment.CurrentDirectory = worktree;
    var vscode = Directory.CreateDirectory(".vscode");
    await Shell.Git.Submodule.Add(repo, "lib", "main");
    var csproj = new FileInfo(Path.Combine("lib", "EverybodyCodes.Core", "EverybodyCodes.Core.csproj"));
    var launch = new FileInfo(Path.Combine("lib", ".vscode", "launch.json"));
    var tasks = new FileInfo(Path.Combine("lib", ".vscode", "tasks.json"));
    var extensions = new FileInfo(Path.Combine("lib", ".vscode", "extensions.json"));
    var settings = new FileInfo(Path.Combine("lib", ".vscode", "settings.json"));

    var text = csproj.ReadToEnd().Replace("'false'", "'true'");
    File.WriteAllText(Path.Combine(worktree, "EverybodyCodes.csproj"), text);
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
    await Shell.VsCode.OpenInNewWindow(worktree);
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
