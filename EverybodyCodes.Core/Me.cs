using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Core;

public sealed record Me(string Name, int Seed, ImmutableDictionary<int, object?> Badges, int Level, [property: JsonConverter(typeof(Me.DateTimeMSConverter))]DateTime ServerTime, [property: JsonConverter(typeof(Me.DateTimeMSConverter))]DateTime PenaltyUntil)
{
    private sealed class DateTimeMSConverter : JsonConverter<DateTime>
    {
        private readonly static DateOnly OriginalDate = new(1970, 1, 1);
        private readonly static TimeOnly OriginalTime = new(1, 0, 0);
        private readonly static DateTime Origin = new(OriginalDate, OriginalTime, DateTimeKind.Utc);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Origin.AddMilliseconds(reader.GetInt64());

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteNumberValue((long)(value - Origin).TotalMilliseconds);
    }

    [JsonIgnore]
    public static Uri BaseAddress { get; } = new("https://everybody.codes");
    public static Uri BaseAddressCdn { get; } = new("https://everybody-codes.b-cdn.net/");
    public CookieContainer Cookies { get; init; } = default!;
    public TimeProvider TimeProvider { get; init; } = TimeProvider.System;
    public TimeSpan PenaltySpan => PenaltyUntil - ServerTime;
    public TimeSpan PenaltyRemaining => PenaltyUntil - DateTime.UtcNow;
    public static async Task<Me> CreateAsync(string cookie)
    {
        var cookieContainer = new CookieContainer();
        cookieContainer.Add(BaseAddress, new Cookie("everybody-codes", cookie));
        cookieContainer.Add(BaseAddressCdn, new Cookie("everybody-codes", cookie));
        using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        using var client = new HttpClient(handler) { BaseAddress = BaseAddress };
        return (await client.GetFromJsonAsync<Me>("/api/user/me"))! with { Cookies = cookieContainer };
    }

    public async Task<Part[]> GetPartsAsync(int year, int day)
    {
        var input = await GetInputAsync(year, day);
        using var handler = new HttpClientHandler() { CookieContainer = Cookies };
        using var client = new HttpClient(handler) { BaseAddress = BaseAddress };
        var keys = (await client.GetFromJsonAsync<Key>($"/api/event/{year}/quest/{day}"))!;

        ImmutableArray<string> answers = [
            ..keys.Answer1 is string a1 ? [a1] : ReadOnlySpan<string>.Empty,
            ..keys.Answer2 is string a2 ? [a2] : ReadOnlySpan<string>.Empty,
            ..keys.Answer3 is string a3 ? [a3] : ReadOnlySpan<string>.Empty
            ];

        return [..keys.Key1 is {} ? [new(this, year, day, 1, DecryptStringFromBytes_Aes(input[1], keys[1, false], keys[1, true]), answers)] : ReadOnlySpan<Part>.Empty,
        ..keys.Key2 is {} ? [new(this, year, day, 2, DecryptStringFromBytes_Aes(input[2], keys[2, false], keys[2, true]), answers)] : ReadOnlySpan<Part>.Empty,
        ..keys.Key3 is {} ? [new(this, year, day, 3, DecryptStringFromBytes_Aes(input[3], keys[3, false], keys[3, true]), answers)] : ReadOnlySpan<Part>.Empty];
    }

    public static IEnumerable<Part> GetTestPart(int year, int day, int part)
    => GetTestParts(year, day)[part - 1];

    public static IEnumerable<Part>[] GetTestParts(int year, int day)
    {
        var input = JsonSerializer.Deserialize<TestInput>(File.ReadAllText(Path.Combine($"D{day:00}", "test.json")))!;

        var p1 = input[1].Select(i => CreatePart(year, day, 1, i));
        var p2 = input[2].Select(i => CreatePart(year, day, 2, i));
        var p3 = input[3].Select(i => CreatePart(year, day, 3, i));
        return [p1, p2, p3];

        static Part CreatePart(int year, int day, int part, TestInput.Part i)
        {
            Globals.Args = i.args?.ToFrozenDictionary();
            return new Part(null!, year, day, part, i.input, [i.answer]);
        }
    }

    static async Task<string> GetJsonAsync(Me me, int year, int day, string endpoint, string? filename = null)
    {
        var path = Path.Combine($"D{day:00}", filename ??= endpoint);
        if (File.Exists(path))
            return File.ReadAllText(path);

        using var handler = new HttpClientHandler() { CookieContainer = me.Cookies };
        using var clientCdn = new HttpClient(handler) { BaseAddress = BaseAddressCdn };

        var json = (await clientCdn.GetStringAsync($"/assets/{year}/{day}/{endpoint}"))!;
        return json;
    }

    public async Task<Input> GetInputAsync(int year, int day)
    {
        if (year > 2000) // Only for quests
        {
            if (day < 1 || day > 20)
                throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 20.");

            var now = TimeProvider.GetUtcNow();
            // Event opens every year on 1st monday of November at 23:00 UTC
            var availableFrom = new DateTimeOffset(year, 11, 1, 23, 0, 0, TimeSpan.Zero).Nth(DayOfWeek.Monday).AddDays(day - 1) // day 1 -> start, day 2 -> start + 1 day, ...
                .AddDays((day - 1) / 5 * 2); // day 5 -> start + 4 day, day 6 -> start + 6 day, ..., day 20 -> start + 25 day

            if (now < availableFrom)
                throw new OutOfDateException(now, availableFrom, day);
        }

        string json = await GetJsonAsync(this, year, day, $"input/{Seed}.json", "input.json");
        var path = Path.Combine($"D{day:00}", "input.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
        CreateSolutionFile(year, day);
        CreateTestFile(day);
        return JsonSerializer.Deserialize<Input>(json)!;

        static void CreateSolutionFile(int year, int day)
        {
            var path = Path.Combine($"D{day:00}", "Solution.cs");
            if (File.Exists(path))
                return;
            File.WriteAllText(path, $$"""
using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;

namespace Y{{year}}.D{{day:00}};

// https://everybody.codes/{{(year < 2000 ? "story" : "event")}}/{{year}}/quests/{{day}}
public sealed class Solution : ISolution
{
    // https://everybody.codes/{{(year < 2000 ? "story" : "event")}}/{{year}}/quests/{{day}}#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    // https://everybody.codes/{{(year < 2000 ? "story" : "event")}}/{{year}}/quests/{{day}}#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    // https://everybody.codes/{{(year < 2000 ? "story" : "event")}}/{{year}}/quests/{{day}}#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}
""");
        }

        static void CreateTestFile(int day)
        {
            var path = Path.Combine($"D{day:00}", "test.json");
            if (File.Exists(path))
                return;
            File.WriteAllText(path, """
{
    "1": [
        {
            "input": "",
            "answer": "",
            "args": null
        }
    ],
    "2": [],
    "3": []
}

""");
        }
    }

    private static string DecryptStringFromBytes_Aes(byte[] input, byte[] key, byte[] iv)
    {
        // Create an Aes object
        // with the specified key and IV.
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        // Create the streams used for decryption.
        using var msDecrypt = new MemoryStream(input);
        using var csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    public sealed record Input([property: JsonPropertyName("1"), JsonConverter(typeof(Input.HexStringConverter))] byte[] A, [property: JsonPropertyName("2"), JsonConverter(typeof(Input.HexStringConverter))] byte[] B, [property: JsonPropertyName("3"), JsonConverter(typeof(Input.HexStringConverter))] byte[] C)
    {
        /// <param name="index">1-based indexing</param>
        public byte[] this[int index] => index switch
        {
            1 => A,
            2 => B,
            3 => C,
            _ => throw new IndexOutOfRangeException(),
        };

        sealed class HexStringConverter : JsonConverter<byte[]>
        {
            public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Convert.FromHexString(reader.GetString()!);

            public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
            => writer.WriteStringValue(Convert.ToHexString(value));
        }
    }

    private sealed record TestInput([property: JsonPropertyName("1")] IEnumerable<TestInput.Part> A, [property: JsonPropertyName("2")] IEnumerable<TestInput.Part> B, [property: JsonPropertyName("3")] IEnumerable<TestInput.Part> C)
    {
        /// <param name="index">1-based indexing</param>
        public IEnumerable<Part> this[int index] => index switch
        {
            1 => A,
            2 => B,
            3 => C,
            _ => throw new IndexOutOfRangeException(),
        };

#pragma warning disable IDE1006 // Naming Styles
        public readonly record struct Part(string input, string answer, IReadOnlyDictionary<string, string>? args);
#pragma warning restore IDE1006 // Naming Styles
    }

    readonly record struct Key(string? Key1, string? Answer1, string? Key2, string? Answer2, string? Key3, string? Answer3)
    {
        public byte[] this[int index, bool iv] => index switch
        {
            1 => GetBytes(Key1 ?? "", iv),
            2 => GetBytes(Key2 ?? "", iv),
            3 => GetBytes(Key3 ?? "", iv),
            _ => throw new IndexOutOfRangeException(),
        };

        private static byte[] GetBytes(string key, bool isIv)
        => isIv ? GetIV(key) : GetKey(key);

        private static byte[] GetKey(string key)
        => Encoding.UTF8.GetBytes(key);
        private static byte[] GetIV(string key)
        => Encoding.UTF8.GetBytes(key[..16]);
    }
}


file static class Ext
{
    extension(DateTimeOffset dto)
    {
        public DateTimeOffset Nth(DayOfWeek day, int nth = 1)
        {
            var fday = new DateTimeOffset(new(dto.Year, dto.Month, 1), new(dto.Hour, dto.Minute), dto.Offset);

            var fOc = fday.DayOfWeek == day ? fday : fday.AddDays(day - fday.DayOfWeek);
            // CurDate = 2011.10.1 Occurance = 1, Day = Friday >> 2011.09.30 FIX.
            if (fOc.Month < dto.Month)
                nth++;
            return fOc.AddDays(7 * (nth - 1));
        }
    }
}

[Serializable]
public sealed class OutOfDateException(DateTimeOffset now, DateTimeOffset availableFrom, int day) : Exception
{
    public DateTimeOffset AvailableFrom { get; } = availableFrom;
    public TimeSpan DurationRemaining { get; } = availableFrom - now;
    public int Day { get; } = day;
    public override string Message
    => $"Day {Day} of {AvailableFrom.Year} is not yet available (available from {AvailableFrom.LocalDateTime:F}, in {DurationRemaining}).";
}
