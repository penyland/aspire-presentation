var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(
    azurite =>
    {
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var blobStorage = storage.AddBlobs("blobs");
var tableStorage = storage.AddTables("tables");

var apiService = builder.AddProject<Projects.AspireStarter_ApiService>("apiservice")
    .WithReference(blobStorage)
    .WithReference(tableStorage)
    .WaitFor(tableStorage)
    .WithEnvironment("MY_ENVIRONMENT_VARIABLE", "HELLO_ASPIRE")
    .WithEnvironment("TABLE_STORAGE_CONNECTION_STRING", () => storage.GetEndpoint("table").Url);

builder.AddProject<Projects.AspireStarter_Web>("webfrontend-blazor")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithEnvironment("MY_ENVIRONMENT_VARIABLE", apiService.GetEndpoint("http"));

builder.AddViteApp("webfrontend-svelte", "../aspire-svelte")
    .WithReference(apiService)
    .WithNpmPackageInstallation();

builder.Build().Run();
