using System.Collections.Immutable;
using System.Net.Http.Json;

sealed record Part(Me Me, int Year, int Day, int PartNo, string Input, ImmutableArray<string> Answers)
{
    public async Task<PartResponse> AnswerAsync(string answer)
    {
        if (Answers.Length >= PartNo && Answers[PartNo - 1] == answer)
            return new(true, true, true, 0, 0, 0, 0, 0);

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

sealed record PartResponse(bool IsCorrect, bool IsLengthCorrect, bool IsFirstCorrect, ulong Time, ulong LocalTime, ulong GlobalTime, int GlobalPlace, int GlobalScore);
