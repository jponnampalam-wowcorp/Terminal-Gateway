using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using Orleans;

namespace TerminalGateway.WebApiSilo
{
    public class ClusterHealthCheck(IMongoClient mongoClient) : IHealthCheck
    {

        public async  Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {

            try
            {
                var adminDatabase = mongoClient.GetDatabase("TerminalGatewayDb");
                var pingCommand = new BsonDocument("ping", 1);
                await adminDatabase.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);
                return new HealthCheckResult(HealthStatus.Healthy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new HealthCheckResult(HealthStatus.Unhealthy, e.Message);
            }
        }
    }
}
        