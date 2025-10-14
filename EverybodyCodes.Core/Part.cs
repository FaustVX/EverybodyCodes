using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

sealed record Part(Me Me, int Year, int Day, int PartNo, string Input, ImmutableArray<string> Answers)
{
    public async Task<PartResponse> AnswerAsync(string answer)
    {
        if (Answers.Length >= PartNo)
            return new(Answers[PartNo - 1] == answer, Answers[PartNo - 1].Length == answer.Length, Answers[PartNo - 1][0] == answer[0], default, default, default, 0, 0);

        using var handler = new HttpClientHandler() { CookieContainer = Me.Cookies };
        using var client = new HttpClient(handler) { BaseAddress = Me.BaseAddress };

        var content = JsonContent.Create(new { answer });
        using var response = (await client.PostAsync($"/api/event/{Year}/quest/{Day}/part/{PartNo}/answer", content))!;
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
        }
        return (await response.Content.ReadFromJsonAsync<PartResponse>())!;
    }
}

sealed record PartResponse([property: JsonPropertyName("correct")] bool IsCorrect, [property: JsonPropertyName("lengthCorrect")] bool IsLengthCorrect, [property: JsonPropertyName("firstCorrect")] bool IsFirstCorrect, [property: JsonConverter(typeof(PartResponse.TimeSpanMSConverter))] TimeSpan Time, [property: JsonConverter(typeof(PartResponse.TimeSpanMSConverter))] TimeSpan LocalTime, [property: JsonConverter(typeof(PartResponse.TimeSpanMSConverter))] TimeSpan GlobalTime, int GlobalPlace, int GlobalScore)
{
    sealed class TimeSpanMSConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeSpan.FromMilliseconds(reader.GetInt64());

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteNumberValue((long)value.TotalMilliseconds);
    }
}
