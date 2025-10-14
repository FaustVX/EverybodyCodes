using System.Net.Http.Json;

sealed record Part(Me Me, int Year, int Day, int PartNo, string Input)
{
    public async Task AnswerAsync(string answer)
    {
        using var handler = new HttpClientHandler() { CookieContainer = Me.Cookies };
        using var client = new HttpClient(handler) { BaseAddress = Me.BaseAddress };

        var content = JsonContent.Create($$"""{"answer":"{{answer}}"}""");
        using var response = (await client.PostAsync($"/api/event/{Year}/quest/{Day}/part/{PartNo}/answer", content))!;
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        // response.EnsureSuccessStatusCode();
    }
}
