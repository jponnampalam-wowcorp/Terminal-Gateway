
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> proj = builder.AddProject<Projects.TerminalGateway_Silo>("terminalgateway-silo");

IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.TerminalGateway_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");




builder.Build().Run();
