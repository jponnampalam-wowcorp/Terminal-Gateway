using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TerminalGateway.WebApiSilo.Controllers
{
    [ApiController]
    [Route("/api/v1/customhealth")]
    public class HealthController : ControllerBase
    {
        [HttpGet]

        public async Task<IActionResult>  GetHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Ok(HealthCheckResult.Healthy());
        }
    }
}
