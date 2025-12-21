using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TerminalGateway.ApiService.Controllers
{

    [ApiController]
    [Route("/api/v1/health")]
    public class HealthController : ControllerBase
    {

        public async Task<HealthCheckResult> GetHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return HealthCheckResult.Healthy();
        }
    }
}
