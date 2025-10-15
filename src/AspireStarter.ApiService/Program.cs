using AspireStarter.ApiService;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Configure Azure Table Storage for Todo items
builder.AddAzureTableServiceClient("AzureTableStorage", settings =>
{
    settings.ConnectionString = builder.Configuration.GetConnectionString("tables");
});

builder.AddRedisOutputCache(connectionName: "cache");

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<TodoService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapOpenApi();
app.MapScalarApiReference("/");

app.UseOutputCache();

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
}).CacheOutput(c =>
{
    c.Expire(TimeSpan.FromSeconds(30));
});

// Todo API Endpoints
app.MapGet("/todos", async (TodoService todoService) =>
{
    var todoEntities = await todoService.GetAllTodosAsync();
    return todoEntities.Select(entity => new TodoDto
    {
        Id = entity.RowKey,
        Title = entity.Title,
        Description = entity.Description,
        IsComplete = entity.IsComplete,
        CreatedAt = entity.CreatedAt
    }).ToList();
});

app.MapGet("/todos/{id}", async (string id, TodoService todoService) =>
{
    var entity = await todoService.GetTodoAsync(id);
    if (entity == null)
        return Results.NotFound();

    return Results.Ok(new TodoDto
    {
        Id = entity.RowKey,
        Title = entity.Title,
        Description = entity.Description,
        IsComplete = entity.IsComplete,
        CreatedAt = entity.CreatedAt
    });
});

app.MapPost("/todos", async (TodoDto todoDto, TodoService todoService) =>
{
    var entity = new TodoEntity
    {
        RowKey = todoDto.Id,
        Title = todoDto.Title,
        Description = todoDto.Description,
        IsComplete = todoDto.IsComplete,
        CreatedAt = todoDto.CreatedAt
    };

    var createdEntity = await todoService.CreateTodoAsync(entity);

    return Results.Created($"/todos/{createdEntity.RowKey}", new TodoDto
    {
        Id = createdEntity.RowKey,
        Title = createdEntity.Title,
        Description = createdEntity.Description,
        IsComplete = createdEntity.IsComplete,
        CreatedAt = createdEntity.CreatedAt
    });
});

app.MapPut("/todos/{id}", async (string id, TodoDto todoDto, TodoService todoService) =>
{
    var entity = await todoService.GetTodoAsync(id);
    if (entity == null)
        return Results.NotFound();

    entity.Title = todoDto.Title;
    entity.Description = todoDto.Description;
    entity.IsComplete = todoDto.IsComplete;

    await todoService.UpdateTodoAsync(entity);
    return Results.NoContent();
});

app.MapDelete("/todos/{id}", async (string id, TodoService todoService) =>
{
    await todoService.DeleteTodoAsync(id);
    return Results.NoContent();
});

app.MapGet("/my-environment-variable", (IConfiguration configuration) =>
{
    var myVariable = configuration["MY_ENVIRONMENT_VARIABLE"] ?? "VARIABLE_NOT_FOUND";
    return Results.Ok(new { MyEnvironmentVariable = myVariable });
});

app.MapGet("/config", ([FromServices] IConfiguration configuration) =>
{
    return (configuration as IConfigurationRoot)?.GetDebugView();
});

app.MapDefaultEndpoints();

app.UseCors();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// DTO for Todo operations
public class TodoDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
