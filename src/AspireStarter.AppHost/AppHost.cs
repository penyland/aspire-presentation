var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(
    azurite =>
    {
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var tableStorage = storage.AddTables("tables");

var apiService = builder.AddProject<Projects.AspireStarter_ApiService>("apiService")
    .WaitFor(tableStorage)
    .WithReference(tableStorage)
    .WithEnvironment("MY_ENVIRONMENT_VARIABLE", "HELLO_WORLD")
    .WithEnvironment("AZURE_TABLE_STORAGE_CONNECTION_STRING", () => storage.GetEndpoint("table").Url);

builder.AddProject<Projects.AspireStarter_Web>("blazorWebFrontend")
    .WithReference(apiService)
    .WithEnvironment("API_BASE_URL", apiService.GetEndpoint("http"));

builder.Build().Run();
