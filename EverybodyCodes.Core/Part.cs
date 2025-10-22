using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Core;

public sealed record Part(Me Me, int Year, int Day, int PartNo, string Input, ImmutableArray<string> Answers)
{
    public async Task<PartResponse> AnswerAsync(string response)
    {
        if (Answers.Length >= PartNo)
            return PartResponse.CreateFromAnswer(Answers[PartNo - 1], response);

        using var handler = new HttpClientHandler() { CookieContainer = Me.Cookies };
        using var client = new HttpClient(handler) { BaseAddress = Me.BaseAddress };

        var content = JsonContent.Create(new { answer = response });
        using var output = (await client.PostAsync($"/api/event/{Year}/quest/{Day}/part/{PartNo}/answer", content))!;
        if (!output.IsSuccessStatusCode)
        {
            Console.WriteLine(await output.Content.ReadAsStringAsync());
            output.EnsureSuccessStatusCode();
        }
        return (await output.Content.ReadFromJsonAsync<PartResponse>())!;
    }
}

public sealed record PartResponse([property: JsonPropertyName("correct")] bool IsCorrect, [property: JsonPropertyName("lengthCorrect")] bool IsLengthCorrect, [property: JsonPropertyName("firstCorrect")] bool IsFirstCorrect, [property: JsonConverter(typeof(PartResponse.TimeSpanMSConverter))] TimeSpan Time, [property: JsonConverter(typeof(PartResponse.TimeSpanMSConverter))] TimeSpan LocalTime, [property: JsonConverter(typeof(PartResponse.TimeSpanMSConverter))] TimeSpan GlobalTime, int GlobalPlace, int GlobalScore)
{
    sealed class TimeSpanMSConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeSpan.FromMilliseconds(reader.GetInt64());

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteNumberValue((long)value.TotalMilliseconds);
    }

    public static PartResponse CreateFromAnswer(string response, string answer)
    => new(response == answer, response.Length == answer.Length, response[0] == answer[0], default, default, default, 0, 0);
}
