using Azure;
using Azure.Data.Tables;

namespace AspireStarter.ApiService;

public class TodoService
{
    private readonly TableClient tableClient;
    private readonly ILogger<TodoService> logger;

    public TodoService(TableServiceClient tableServiceClient, ILogger<TodoService> logger)
    {
        tableServiceClient.CreateTableIfNotExists("todos");
        tableClient = tableServiceClient.GetTableClient("todos");

        this.logger = logger;
    }

    public async Task<List<TodoEntity>> GetAllTodosAsync()
    {
        try
        {
            var todos = new List<TodoEntity>();
            var queryResults = tableClient.QueryAsync<TodoEntity>();

            await foreach (var todo in queryResults)
            {
                todos.Add(todo);
            }

            return todos;
        }
        catch (Exception)
        {
            logger?.LogWarning("Failed to retrieve todos from the table storage.");
            return [];
        }
    }

    public async Task<TodoEntity?> GetTodoAsync(string id)
    {
        try
        {
            return await tableClient.GetEntityAsync<TodoEntity>("todo", id);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger?.LogWarning("Todo with id: {Id} not found", id);
            return null;
        }
    }

    public async Task<TodoEntity> CreateTodoAsync(TodoEntity todo)
    {
        try
        {
            todo.PartitionKey = "todo";
            todo.RowKey = string.IsNullOrEmpty(todo.RowKey) ? Guid.NewGuid().ToString() : todo.RowKey;
            todo.Timestamp = DateTimeOffset.UtcNow;
            todo.CreatedAt = DateTimeOffset.UtcNow;

            await tableClient.UpsertEntityAsync(todo);
            return todo;
        }
        catch (Exception)
        {
            logger?.LogWarning("Failed to create todo with title: {Title}", todo.Title);
            throw;
        }
    }

    public async Task UpdateTodoAsync(TodoEntity todo)
    {
        todo.PartitionKey = "todo";
        await tableClient.UpdateEntityAsync(todo, ETag.All, TableUpdateMode.Replace);
    }

    public async Task DeleteTodoAsync(string id)
    {
        try
        {
            await tableClient.DeleteEntityAsync("todo", id);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogWarning("Attempted to delete a non-existent todo with id: {Id}", id);
            throw;
        }
    }
}

public class TodoEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "todo";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
