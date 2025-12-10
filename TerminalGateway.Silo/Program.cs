// See https://aka.ms/new-console-template for more information

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
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables()
        .Build();

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    builder.Host
        .UseOrleans((context, silo) =>
        {
            silo
             .ConfigureLogging(logging => logging.AddConsole())
             .UseLocalhostClustering()
             .AddMemoryGrainStorage("licenses")
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

        });

    WebApplication app = builder.Build();
    app.Run();

}
