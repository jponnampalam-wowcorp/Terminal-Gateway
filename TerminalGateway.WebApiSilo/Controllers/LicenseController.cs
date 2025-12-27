using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TerminalGateway.WebApiSilo.Controllers
{

    [ApiController]
    [Route("api/v1/license")]
    public class LicenseController : ControllerBase
    {
        private readonly IClusterClient _client;
        public LicenseController(IClusterClient client)
        {
            _client = client;
        }
        [HttpGet]
        public async Task<IActionResult> GetLicensesAsync([FromQuery] int page = 0, [FromQuery] int size = 50)
        {

            string licenseText = $"Test License. Page={page}, Size = {size}";
            
            return Ok($"License data is {licenseText}");
        }
    }
}
