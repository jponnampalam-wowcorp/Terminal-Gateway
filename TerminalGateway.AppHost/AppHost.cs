using Aspire.Hosting.Orleans;
using Microsoft.Extensions.Configuration;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "TerminalGateway__");


string? databaseName = builder.Configuration.GetValue<string>("Mongo:DatabaseName");

IResourceBuilder<MongoDBServerResource> mongo = builder.AddMongoDB("mongo");
IResourceBuilder<MongoDBDatabaseResource> mongoDb = mongo.AddDatabase(databaseName);

OrleansService orleans = builder.AddOrleans("default").WithClustering(mongoDb);



////Step 5: Add Silo project
//IResourceBuilder<ProjectResource> silo = builder
//    .AddProject<Projects.TerminalGateway_Silo>("terminalgateway-silo")
//    .WaitFor(mongoDb)
//    .WithReference(mongoDb);
//.WithHttpEndpoint();


//Step 6: Add API Service project
//IResourceBuilder<ProjectResource> apiService = builder
//    .AddProject<Projects.TerminalGateway_ApiService>("apiservice")
//    .WaitFor(mongoDb)
//    .WithReference(mongoDb)
//    .WithReference(silo)
//    .WithReference(orleans);



builder.AddProject<Projects.TerminalGateway_WebApiSilo>("terminalgateway-webapisilo")
    .WaitFor(mongoDb)
    .WithReference(mongoDb)
    .WithHttpHealthCheck(path: "/health");
//.WithHttpEndpoint();


////Step 6: Add API Service project
//IResourceBuilder<ProjectResource> apiService = builder
//    .AddProject<Projects.TerminalGateway_ApiService>("apiservice")
//    .WaitFor(mongoDb)
//    .WithReference(mongoDb)
//    .WithReference(orleans);







//Step 6: Add API Service project
IResourceBuilder<ProjectResource> apiService = builder
    .AddProject<Projects.TerminalGateway_ApiService>("apiservice")
    .WaitFor(mongoDb)
    .WithReference(mongoDb)
    .WithReference(orleans);



builder.Build().Run();


