var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireStarter_ApiService>("apiService");

builder.Build().Run();
