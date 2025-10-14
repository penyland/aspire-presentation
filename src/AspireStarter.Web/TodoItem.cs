namespace AspireStarter.Web;

public class TodoItem
{
    public string Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
