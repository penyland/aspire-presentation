var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(
    azurite =>
    {
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var tableStorage = storage.AddTables("tables");

var cache = builder.AddRedis("cache")
                   .WithRedisInsight()
                   .WithLifetime(ContainerLifetime.Persistent);

var chuckApi = builder.AddExternalService("chuckapi", "https://api.chucknorris.io")
    .WithHttpHealthCheck("/");

var apiService = builder.AddProject<Projects.AspireStarter_ApiService>("apiService")
    .WaitFor(tableStorage)
    .WithReference(tableStorage)
    .WithReference(cache)
    .WithReference(chuckApi)
    .WithEnvironment("MY_ENVIRONMENT_VARIABLE", "HELLO_WORLD")
    .WithEnvironment("AZURE_TABLE_STORAGE_CONNECTION_STRING", () => storage.GetEndpoint("table").Url);

builder.AddProject<Projects.AspireStarter_Web>("webFrontend-blazor")
    .WithReference(apiService)
    .WithEnvironment("API_BASE_URL", apiService.GetEndpoint("http"));

var svelteApp = builder.AddViteApp("webfrontend-svelte", "../aspire-svelte")
    .WithReference(apiService)
    .WithNpmPackageInstallation()
    .WithEnvironment("API_BASE_URL", apiService.GetEndpoint("http"));

builder.Build().Run();
