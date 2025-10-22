using System.Reflection;
using Cocona;
using EverybodyCodes.Core;

var app = CoconaLiteApp.Create(args);

app.AddCommand("run", async ([Argument] int year, [Argument] int day, [Argument] int part, [Option('s')] string session) =>
{
    var me = await Me.CreateAsync(session);
    var input = await me.GetInputAsync(year, day, part);
    var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
    var solution = (ISolution)Activator.CreateInstance(type!)!;
    var output = solution.Solve(part, input.Input);
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
    var type = Assembly.GetEntryAssembly()!.GetType($"Y{year}.D{day:00}.Solution");
    var solution = (ISolution)Activator.CreateInstance(type!)!;
    var output = solution.Solve(part, input.Input);
    Console.WriteLine(output);
});

app.Run();
