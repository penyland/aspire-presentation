namespace AspireStarter.ApiService;

public static class ChuckApiHttpClientExtensions
{
    public static void AddChuckApiHttpClient(this IServiceCollection services) =>
        services.AddHttpClient<ChuckApiHttpClient>(client =>
        {
            client.BaseAddress = new("https+http://chuckapi");
        });
}

public class ChuckApiHttpClient(HttpClient httpClient)
{
    public async Task<string> GetRandomJokeAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<ChuckApiResponse>("jokes/random", cancellationToken);
        return response?.Value ?? "No joke found.";
    }

    private record ChuckApiResponse(string Id, string IconUrl, string Url, string Value);
}
