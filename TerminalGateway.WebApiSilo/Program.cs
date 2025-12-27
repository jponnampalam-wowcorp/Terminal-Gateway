using k8s.KubeConfigModels;
using Orleans.Configuration;
using Serilog;
using System.Collections;
using TerminalGateway.ServiceDefaults;
using TerminalGateway.WebApiSilo;

IDictionary environmentVariables = Environment.GetEnvironmentVariables();
foreach (DictionaryEntry dictionaryEntry in environmentVariables)
{
    string key = dictionaryEntry.Key.ToString()!;
    if (key.StartsWith("Orleans__GrainStorage__") ||
        key.StartsWith("Orleans__Clustering__"))
    {
        Environment.SetEnvironmentVariable(key, null);
    }
}

try
{
    StartSilo(args);
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return 1;
}

static void StartSilo(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables(prefix: "TerminalGateway__");
    builder.AddServiceDefaults();
    builder.Services.AddHealthChecks()
        .AddCheck<ClusterHealthCheck>("OrleansClusterHealthCheck");

    builder.AddMongoDBClient("mongo");

    // Add services to the container.

    builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Host
        .UseOrleans((context, silo) =>
        {
            string? connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:mongo");
            // string? connectionString = context.Configuration.GetConnectionString("mongodb");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MongoDBConnection string is not configured.");
            }
            silo
                .ConfigureLogging(logging => logging.AddConsole())
                .UseMongoDBClient(connectionString)
                .AddMongoDBGrainStorage(
                    name:"UserStorage",
                    options=>
                {
                    options.DatabaseName = "TerminalGatewayDb";
                    // Optional: further configuration, e.g., CreateShardKeyForCosmos if using Azure CosmosDB with Mongo API
                    options.CreateShardKeyForCosmos = false;
                    
                })

                .UseMongoDBClustering(options => { options.DatabaseName = "TerminalGatewayDb"; })
                .UseMongoDBReminders(options =>
                {
                    options.DatabaseName = "TerminalGatewayDb";
                    ;
                })
                .AddActivityPropagation()

                .Configure<ClusterOptions>(options =>
                {

                    options.ClusterId = "dev";
                    options.ServiceId = "LicenseServerApp";
                })
                .UseDashboard(options =>
                {
                    options.Username = "username";
                    options.Password = "password";
                    options.Host = "*";
                    options.Port = 9001;
                    options.HostSelf = true;
                    options.CounterUpdateIntervalMs = 1000;

                });

        })
        .UseSerilog((hostContext, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostContext.Configuration)
                .Enrich.FromLogContext();
               // .WriteTo.OpenTelemetry();
        });


    var app = builder.Build();
    app.MapHealthChecks("/health");
    
    app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();


    ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("Host built successfully. Silo starting at {Time}", DateTime.UtcNow);

    

    app.Run();
}
