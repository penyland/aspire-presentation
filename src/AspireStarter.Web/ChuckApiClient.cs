namespace AspireStarter.ApiService;

public static class ChuckApiHttpClientExtensions
{
    public static void AddChuckApiClient(this IServiceCollection services) =>
        services.AddHttpClient<ChuckApiClient>(client =>
        {
            client.BaseAddress = new("https+http://apiservice");
        });
}

public class ChuckApiClient(HttpClient httpClient)
{
    public async Task<string> GetRandomJokeAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<ChuckJokeResponse>("chuckjoke", cancellationToken);
        return response?.Joke ?? "No joke found.";
    }

    private record ChuckJokeResponse(string Joke);
}
