using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using Terminal.Gateway.Grains;

namespace TerminalGateway.ApiService
{
    public class ApiHealthCheck(IClusterClient clusterClient) : IHealthCheck
    {

        public async  Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                // Get a reference to the local stateless worker grain and call its method
                var grain = clusterClient.GetGrain<IHealthCheckGrain>(Guid.Empty);
                var isHealthy = await grain.CheckHealthAsync();

                return isHealthy ? HealthCheckResult.Healthy("Orleans silo is healthy and responding to grain calls.") : HealthCheckResult.Unhealthy("Orleans silo is unhealthy.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Orleans silo health check failed with an exception.", ex);
            }
        }
    }
}
