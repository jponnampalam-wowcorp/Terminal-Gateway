using Microsoft.Orleans.Providers.Mongo.Utils;
using MongoDB.Driver;

namespace TerminalGateway.ApiService
{
    public sealed class CustomMongoClientFactory : IMongoClientFactory
    {
        private readonly ILogger<CustomMongoClientFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        // Inject any required services (e.g., IOptions<T> for configuration, ILogger)
        public CustomMongoClientFactory(ILogger<CustomMongoClientFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IMongoClient Create(string name)
        {

            // Example: Get configuration from IOptions<T>
            // var config = _serviceProvider.GetRequiredService<IOptions<MyMongoDbConfiguration>>().Value;
            // var settings = MongoClientSettings.FromConnectionString(config.ConnectionString);

            // A simple example using a hardcoded connection string for demonstration
            string connectionString = "mongodb://localhost:27017";
            _logger.LogInformation("Creating MongoClient for name {ClientName} using connection string {ConnString}",
                name, connectionString);

            return new MongoClient(connectionString);
        }

    }
}
