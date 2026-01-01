using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Orleans.Configuration;
using Serilog;
using System.Collections;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using TerminalGateway.ApiService;
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

builder.Services.AddHealthChecks()
    .AddCheck<ApiHealthCheck>("OrleansClientHealthCheck");


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


    });
   


builder.Services.Configure<MongoClientSettings>(options =>
{
    options.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.RespectRequiredConstructorParameters = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Add services to the container.
builder.Services.AddProblemDetails(options =>
{

    options.CustomizeProblemDetails = context =>
    {
       context.ProblemDetails.Instance=  $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
       context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
       var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
       context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
       context.ProblemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
    };
});

builder.Host.UseSerilog((hostContext, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostContext.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.OpenTelemetry();
});


//builder.Services.AddLifecycleHook<GetMongoResourceLogsLifecycleHook>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


WebApplication app = builder.Build();

app.MapHealthChecks("/health");



app.UseExceptionHandler("/error");

app.Map("/error", async (HttpContext httpContext) =>
{
    var problemDetails = new ProblemDetails
    {
        Type = "/errors/UnknownError", // Custom error type
        Title = "An unexpected error occurred.",
        Status = (int)HttpStatusCode.InternalServerError,
        Detail = "Something went wrong. Please try again later.",
        Instance = httpContext.Request.Path // Identifies where the error occurred
    };

    httpContext.Response.ContentType = "application/json";
    httpContext.Response.StatusCode = problemDetails.Status.Value;
    await httpContext.Response.WriteAsJsonAsync(problemDetails);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();





