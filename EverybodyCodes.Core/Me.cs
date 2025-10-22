using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Core;

public sealed record Me(string Name, int Seed, ImmutableDictionary<int, object?> Badges, int Level)
{
    [JsonIgnore]
    public static Uri BaseAddress { get; } = new("https://everybody.codes");
    public static Uri BaseAddressCdn { get; } = new("https://everybody-codes.b-cdn.net/");
    public CookieContainer Cookies { get; init; } = default!;
    public static async Task<Me> CreateAsync(string cookie)
    {
        var cookieContainer = new CookieContainer();
        cookieContainer.Add(BaseAddress, new Cookie("everybody-codes", cookie));
        cookieContainer.Add(BaseAddressCdn, new Cookie("everybody-codes", cookie));
        using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        using var client = new HttpClient(handler) { BaseAddress = BaseAddress };
        return (await client.GetFromJsonAsync<Me>("/api/user/me"))! with { Cookies = cookieContainer };
    }

    public async Task<Part> GetInputAsync(int year, int day, int part)
    {
        var input = await GetInputAsync(year, day);
        using var handler = new HttpClientHandler() { CookieContainer = Cookies };
        using var client = new HttpClient(handler) { BaseAddress = BaseAddress };
        var keys = (await client.GetFromJsonAsync<Key>($"/api/event/{year}/quest/{day}"))!;

        return new(this, year, day, part, DecryptStringFromBytes_Aes(input, keys, part), [
            ..keys.Answer1 is string a1 ? new ReadOnlySpan<string>(ref a1) : [],
            ..keys.Answer2 is string a2 ? new ReadOnlySpan<string>(ref a2) : [],
            ..keys.Answer3 is string a3 ? new ReadOnlySpan<string>(ref a3) : []
            ]);
    }

    public static Part GetTestInputAsync(int year, int day, int part, string file)
    {
        var input = JsonSerializer.Deserialize<Input>(File.ReadAllText(file))!;

        return new(null!, year, day, part, Encoding.UTF8.GetString(input[part]), []);
    }

    public async Task<Input> GetInputAsync(int year, int day)
    {
        return JsonSerializer.Deserialize<Input>(await GetJson(this, year, day))!;

        static async Task<string> GetJson(Me me, int year, int day)
        {
            var path = Path.Combine("Solutions", $"Y{year}", $"D{day:00}", "input.json");
            if (File.Exists(path))
                return File.ReadAllText(path);

            using var handler = new HttpClientHandler() { CookieContainer = me.Cookies };
            using var clientCdn = new HttpClient(handler) { BaseAddress = BaseAddressCdn };

            var json = (await clientCdn.GetStringAsync($"/assets/{year}/{day}/input/{me.Seed}.json"))!;
            CreateParentDir(Path.GetDirectoryName(path)!);
            CreateSolutionFile(year, day);
            File.WriteAllText(path, json);
            return json;

            static void CreateParentDir(string dir)
            {
                if (string.IsNullOrWhiteSpace(dir) || Directory.Exists(dir))
                    return;
                CreateParentDir(Path.GetDirectoryName(dir)!);
                Directory.CreateDirectory(dir);
            }
        }

        static void CreateSolutionFile(int year, int day)
        {
            var path = Path.Combine("Solutions", $"Y{year}", $"D{day:00}", "Solution.cs");
            if (File.Exists(path))
                return;
            File.WriteAllText(path, $$"""
using ZLinq;

namespace Y{{year}}.D{{day:00}};

public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    public string Solve2(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}
""");
        }
    }

    private static string DecryptStringFromBytes_Aes(Input input, Key key, int part)
    {
        // Create an Aes object
        // with the specified key and IV.
        using var aes = Aes.Create();
        aes.Key = key[part, false];
        aes.IV = key[part, true];

        // Create the streams used for decryption.
        using var msDecrypt = new MemoryStream(input[part]);
        using var csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    public sealed record Input([property: JsonPropertyName("1"), JsonConverter(typeof(Input.HexStringConverter))] byte[] A, [property: JsonPropertyName("2"), JsonConverter(typeof(Input.HexStringConverter))] byte[] B, [property: JsonPropertyName("3"), JsonConverter(typeof(Input.HexStringConverter))] byte[] C)
    {
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
            {
                var r = reader;
                try
                {
                    return Convert.FromHexString(reader.GetString()!);
                }
                catch (FormatException)
                {
                    return Encoding.UTF8.GetBytes(r.GetString()!);
                }
            }

            public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
            => writer.WriteStringValue(Convert.ToHexString(value));
        }
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
