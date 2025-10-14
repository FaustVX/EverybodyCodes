Console.WriteLine("Hello, World!");

var me = await Me.CreateAsync("");
var part1 = await me.GetInputAsync(2024, 1, 1);
await part1.AnswerAsync(0.ToString());
