using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TerminalGateway.WebApiSilo
{
    public class ClusterHealthCheck: IHealthCheck
    {
        private readonly IClusterClient _client;

        public ClusterHealthCheck(IClusterClient client)
        {
            _client = client;
        }
        public async  Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return HealthCheckResult.Healthy();
        }
    }
}
        