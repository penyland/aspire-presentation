var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireStarter_ApiService>("apiService");

builder.AddProject<Projects.AspireStarter_Web>("blazorWebFrontend")
    .WithReference(apiService);

builder.Build().Run();
