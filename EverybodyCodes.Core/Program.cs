using System.Reflection;
using Cocona;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Attributes;

var app = CoconaLiteApp.Create(args);

app.AddCommand("run", async ([Argument] int year, [Argument] int day, [Argument] int part, [Option('s')] string session) =>
{
    var me = await Me.CreateAsync(session);
    var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution")!;
    Globals.AddFinalLineFeed[part] = type.GetMethod($"Solve{part}")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
    var input = await me.GetInputAsync(year, day, part);
    var solution = (ISolution)Activator.CreateInstance(type)!;
    var startTime = TimeProvider.System.GetTimestamp();
    var output = solution.Solve(part, input.Input);
    Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
    Console.WriteLine(output);
    var response = await input.AnswerAsync(output);
    Console.WriteLine(response);
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
});

app.AddCommand("test", ([Argument] int year, [Argument] int day, [Argument] int part, [Argument]string file) =>
{
    var input = Me.GetTestInputAsync(year, day, part, file);
    var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution")!;
    var solution = (ISolution)Activator.CreateInstance(type)!;
    var startTime = TimeProvider.System.GetTimestamp();
    var output = solution.Solve(part, input.Input);
    Console.WriteLine(TimeProvider.System.GetElapsedTime(startTime));
    Console.WriteLine(output);
});

app.AddCommand("bench", async ([Argument] int year, [Argument] int day, [Option('s')] string session) =>
{
    var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution")!;
    var addFinalLF1 = type.GetMethod("Solve1")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
    var addFinalLF2 = type.GetMethod("Solve2")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
    var addFinalLF3 = type.GetMethod("Solve3")!.CustomAttributes.Any(a => a.AttributeType == typeof(AddFinalLineFeedAttribute));
    Globals.AddFinalLineFeed[1] = addFinalLF1;
    Globals.AddFinalLineFeed[2] = addFinalLF2;
    Globals.AddFinalLineFeed[3] = addFinalLF3;
    var bench = type.CustomAttributes.Any(a => a.AttributeType == typeof(ShortBenchmarkAttribute)) ? typeof(ShortBench<>) : typeof(LongBench<>);
    bench = bench.MakeGenericType(type);
    var me = await Me.CreateAsync(session);
    var input1 = await me.GetInputAsync(year, day, 1);
    var input2 = await me.GetInputAsync(year, day, 2);
    var input3 = await me.GetInputAsync(year, day, 3);
    Globals.Parts[1] = input1;
    Globals.Parts[2] = input2;
    Globals.Parts[3] = input3;
    BenchmarkDotNet.Running.BenchmarkRunner.Run(bench);
    // File.Copy($"BenchmarkDotNet.Artifacts/results/*.md", Path.Combine($"D{day:00}", "benchmark.md"), overwrite: true);
});

app.Run();
