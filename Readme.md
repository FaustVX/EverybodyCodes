# EverybodyCodes

Lightweight command-line runner and helpers for [Everybody.Codes](https://everybody.codes/) puzzles and events.

## Overview

This repository contains the core runner, utilities and CLI commands used to fetch inputs, run solutions, test locally and scaffold new year worktrees.

## Quickstart

Prerequisites:
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Everybody.Codes](https://everybody.codes/)  account session cookie

Build:
```sh
dotnet build EverybodyCodes.Core
```

Fetch inputs:
```sh
dotnet run -- get 2025 01 --session <SESSION_COOKIE>
```

Run a solution and submit:
```sh
dotnet run -- run 2025 01 1 --session <SESSION_COOKIE>
```

Run tests from a local test file:
```sh
dotnet run -- test 2025 01 1 --file D01/test1.json
```

Run the 3 tests from a local test file:
```sh
dotnet run -- test 2025 01 --file D01/test1.json
```

## The `new` command (scaffold a new year)

Implemented in the CLI runner: see [`EverybodyCodes.Core.Program`](EverybodyCodes.Core/Program.cs). Use it to create a new per-year worktree, add the core repo as a submodule, copy VS Code configs and scaffold a project for the year's solutions.

Usage:
```sh
dotnet run -- new <year> [-r <git repo> ] [-b <branch>]
# examples
dotnet run -- new 2026
dotnet run -- new 2026 -r git@github.com:YourUser/EverybodyCodes.git -b main
```

What it does
- Creates a git worktree at ../<year> and makes it an orphan branch (via Shell.Git.Worktree.Add) — see [EverybodyCodes.Core.Shell](EverybodyCodes.Core/Shell.cs).
- Adds the core repository as a submodule at `lib`.
- Copies and adapts VS Code launch/tasks configs from `lib/.vscode` to the new worktree (modifies program/project paths and the year input).
- Creates a per-year solution/project and wires it into a solution file.
- Opens the new worktree in a new VS Code window.

Files and behavior referenced:
- CLI implementation: [EverybodyCodes.Core/Program.cs](EverybodyCodes.Core/Program.cs)
- Worktree/submodule helpers: [EverybodyCodes.Core/Shell.cs](EverybodyCodes.Core/Shell.cs)
- Per-day scaffolding (when fetching inputs): [EverybodyCodes.Core.Me.CreateSolutionFile](EverybodyCodes.Core/Me.cs) and [EverybodyCodes.Core.Me.CreateTestFile](EverybodyCodes.Core/Me.cs)

## How solutions work (short)

- Solutions implement `ISolution` ([EverybodyCodes.Core/ISolution.cs](EverybodyCodes.Core/ISolution.cs)) with Solve1/2/3 methods.
- The runner instantiates `Y{year}.D{day:00}.Solution` and calls `ISolution.Solve(part, input)`; see [EverybodyCodes.Core/Program.cs](EverybodyCodes.Core/Program.cs).
- Mark methods with `[AddFinalLineFeed]` (see [EverybodyCodes.Core.Attributes.AddFinalLineFeedAttribute](EverybodyCodes.Core/Attributes/AddFinalLineFeedAttribute.cs)) when the solution's input needs a trailing newline added before processing; especially usefull for (RO)Span2D<char>

## Using SpanIndexer<T>

SpanIndexer provides a zero-allocation, indexable view over tokens (ranges) of a ReadOnlySpan<T>. Typical pattern:

```csharp
// split a line into token Ranges stored in a stack buffer,
// then create an indexer view that returns ReadOnlySpan<char> tokens.
ReadOnlySpan<char> input = "apple:10;banana:20;cherry:30";

var parts = (stackalloc Range[2]);
foreach (var lineRange in input.SplitAny(';'))
{
    switch (parts[..input[lineRange].SplitAny(parts, [':'], StringSplitOptions.RemoveEmptyEntries)].ToIndexer(input[lineRange]))
    {
        case ["apple", var apple]:
            break;
        case ["banana", var banana]:
            break;
        case ["cherry", var cherry]:
            break;
    }
}
```

Notes:
- SplitAny fills the provided Range buffer and returns the number of tokens written.
- ToIndexer(ranges, originalSpan) (used above as an extension) returns an indexable view whose elements are ReadOnlySpan<T>, enabling parsing and indexing without allocating.

## Development notes

- Submit answers via [`EverybodyCodes.Core.Part.AnswerAsync`](EverybodyCodes.Core/Part.cs).
- Inputs are downloaded & cached under `D{day:00}/input.json` (see [EverybodyCodes.Core.Me.GetInputAsync](EverybodyCodes.Core/Me.cs)).
- Use the included VS Code tasks and launch configurations in `.vscode/` to build, fetch, run and test.

Key code:
- Runner & commands: [`EverybodyCodes.Core.Program`](EverybodyCodes.Core/Program.cs)
- Account / input handling: [`EverybodyCodes.Core.Me`](EverybodyCodes.Core/Me.cs)
- Answer submission: [`EverybodyCodes.Core.Part`](EverybodyCodes.Core/Part.cs)
- Shell helpers (git/dotnet/vscode): [`EverybodyCodes.Core.Shell`](EverybodyCodes.Core/Shell.cs)
- Attributes: [`EverybodyCodes.Core.Attributes.AddFinalLineFeedAttribute`](EverybodyCodes.Core/Attributes/AddFinalLineFeedAttribute.cs)
- Helpers / extensions: [`EverybodyCodes.Core.Extensions.SpanIndexer<T>`](EverybodyCodes.Core/Extensions/SpanIndexer.cs)

## Contributing

Add daily solutions under `D{day:00}/` implementing `ISolution`. Use the `get`, `run`, `test` and `new` commands to workflow and scaffold new years.

License: MIT.
