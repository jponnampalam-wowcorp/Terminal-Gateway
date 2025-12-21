using Orleans.Configuration;
using System.Collections;
using System.Text.Json.Serialization;
using TerminalGateway.ServiceDefaults;



IDictionary environmentVariables = Environment.GetEnvironmentVariables();
foreach (DictionaryEntry dictionaryEntry in environmentVariables)
{
    string key = dictionaryEntry.Key.ToString()!;
    if (key.StartsWith("Orleans__GrainStorage__Default") ||
        key.StartsWith("Orleans__Clustering__"))
    {
        Environment.SetEnvironmentVariable(key, null);
    }
}
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(prefix: "TerminalGateway__");

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.AddMongoDBClient("mongo");


builder.Host
    .UseOrleansClient((context, clientBuilder) =>
    {
        string? connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:Mongo");
        //string? connectionString = context.Configuration.GetConnectionString("mongo");
        clientBuilder.UseMongoDBClient(connectionString);

        //clientBuilder.Services.AddSingleton<IMongoClient>(sp =>
        //    sp.GetRequiredService<IMongoClientFactory>().Create("default"));
        clientBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "LicenseServerApp";
            })
        .UseMongoDBClustering(options =>
        {
            options.DatabaseName = "TerminalGatewayDb";
        });


    })
    .ConfigureLogging(logging => logging.AddConsole());

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

IClusterClient client = app.Services.GetRequiredService<IClusterClient>();
// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();

