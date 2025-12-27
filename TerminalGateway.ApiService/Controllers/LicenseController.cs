using System.Runtime.CompilerServices;
using Terminal.Gateway.Grains;
using WPayApps.Licenses.GrainInterfaces;

namespace TerminalGateway.ApiService.Controllers
{
    [ApiController]
    [Route("api/v1/license")]
    public class LicenseController : ControllerBase
    {
        private readonly IClusterClient _client;
        private ILogger<LicenseController> _logger;

        public LicenseController(IClusterClient client, ILogger<LicenseController> logger)
        {
            _client = client;
            _logger= logger;
        }

        [HttpPost("create")]
        public Task<IActionResult> AddUser([FromBody] User userProfile)
        {

            var res = _client.GetGrain<IUserGrain>(userProfile.UserId);
            res.AddUser(new UserProfile
            { Email = userProfile.Email, FirstName = userProfile.FirstName, LastName = userProfile.LastName });
            _logger.LogInformation("user  with email  {email} created/updated", userProfile.Email);
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpPost("update")]
        public Task<IActionResult> UpdateUser([FromBody] User userProfile)
        {

            var res = _client.GetGrain<IUserGrain>(userProfile.UserId);
            res.Update(new UserProfile
                { Email = userProfile.Email, FirstName = userProfile.FirstName, LastName = userProfile.LastName });
            _logger.LogInformation("user  with email  {email} created/updated", userProfile.Email);
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetUser([FromQuery] string userId)
        {
            try
            {
                var res = _client.GetGrain<IUserGrain>(userId);
                var profile = await res.GetUserProfile();
                var user = new User
                {
                    UserId = userId,
                    Email = profile.Email,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName
                };

                return Ok(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Problem("User not found", statusCode: 404);  
            }

        }
    }
}
