namespace AspireStarter.Web;

public static class TodoApiClientExtensions
{
    public static void AddTodoApiClient(this IServiceCollection services) =>
        services.AddHttpClient<TodoApiClient>(client =>
        {
            // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
            // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
            client.BaseAddress = new("https+http://apiservice");
        });
}

public class TodoApiClient(HttpClient httpClient)
{
    public async Task<List<TodoItem>> GetTodosAsync(CancellationToken cancellationToken = default)
    {
        var todos = await httpClient.GetFromJsonAsync<List<TodoItem>>("/todos", cancellationToken) 
            ?? [];
        return todos;
    }

    public async Task<TodoItem?> GetTodoAsync(string id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TodoItem>($"/todos/{id}", cancellationToken);
    }

    public async Task<TodoItem?> CreateTodoAsync(TodoItem todo, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/todos", new TodoItemRequestDto()
        {
            Title = todo.Title,
            Description = todo.Description,
            IsComplete = todo.IsComplete
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoItem>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateTodoAsync(TodoItem todo, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/todos/{todo.Id}", todo, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTodoAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/todos/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

record TodoItemRequestDto 
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
}
