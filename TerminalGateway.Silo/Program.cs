// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections;


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
    //IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
    //string? connectionString = null;
    //hostBuilder.ConfigureAppConfiguration((hostContext, config) =>
    //    {
    //        config.AddEnvironmentVariables("TerminalGateway__");
    //    })
    //    .ConfigureServices((hostContext, services) =>
    //    {
    //        // The IHostEnvironment is also available here
    //        connectionString = hostContext.Configuration.GetValue<string>("ConnectionStrings:Mongo");
    //    });

    //hostBuilder.UseOrleans(siloBuilder =>
    //    {
    //        if (string.IsNullOrEmpty(connectionString))
    //        {
    //            throw new InvalidOperationException("MongoDBConnection string is not configured.");
    //        }

    //        siloBuilder.ConfigureLogging(logging => logging.AddConsole());
    //        siloBuilder.UseMongoDBClient(connectionString);
    //        siloBuilder.AddMongoDBGrainStorageAsDefault(options =>
    //        {
    //            options.DatabaseName = "TerminalGatewayDb";
    //        });

    //        siloBuilder.UseMongoDBClustering(options =>
    //        {

    //            options.DatabaseName = "TerminalGatewayDb";
    //            options.Strategy = MongoDBMembershipStrategy.Multiple;


    //        });
    //        siloBuilder.UseMongoDBReminders(options =>
    //            {
    //                options.DatabaseName = "TerminalGatewayDb"; ;
    //            });
    //        siloBuilder.AddActivityPropagation();
    //        siloBuilder.Configure<ClusterOptions>(options =>
    //         {

    //             options.ClusterId = "dev";
    //             options.ServiceId = "LicenseServerApp";
    //         })
    //        .UseDashboard(options =>
    //        {
    //            options.Username = "username";
    //            options.Password = "password";
    //            options.Host = "*";
    //            options.Port = 9001;
    //            options.HostSelf = true;
    //            options.CounterUpdateIntervalMs = 1000;

    //        });
    //    })
    //    .UseSerilog((hostContext, services, loggerConfiguration) =>
    //    {
    //        loggerConfiguration
    //            .ReadFrom.Configuration(hostContext.Configuration)
    //            .Enrich.FromLogContext();
    //    });

    //using IHost host = hostBuilder.Build();
    //ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
    //ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

    //logger.LogInformation("Host built successfully. Application starting...");
    //host.RunAsync();
    //IHostBuilder builder = Host.CreateApplicationBuilder(args);
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables(prefix: "TerminalGateway__");
    builder.Host
        .UseOrleans((context, silo) =>
        {
            string? connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:mongo");
            // string? connectionString = context.Configuration.GetConnectionString("mongodb");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MongoDBConnection string is not configured.");
            }

            // silo.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
            //silo.Services.AddSingleton<IMongoClientFactory, CustomMongoClientFactory>();
            //silo.Services.AddSingleton<IMongoClient>(sp =>
            //    sp.GetRequiredService<IMongoClientFactory>().Create("default"));
            silo
                .ConfigureLogging(logging => logging.AddConsole())
                .UseMongoDBClient(connectionString)
                .AddMongoDBGrainStorageAsDefault(options =>
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
                });

    WebApplication app = builder.Build();

    ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("Host built successfully. Application starting...");
    app.Run();
}
