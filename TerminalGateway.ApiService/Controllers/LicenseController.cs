using Terminal.Gateway.Grains;
using WPayApps.Licenses.GrainInterfaces;

namespace TerminalGateway.ApiService.Controllers
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

        [HttpPost]
        public Task<IActionResult> AddUser([FromBody] User userProfile)
        {

            var res = _client.GetGrain<IUserGrain>(userProfile.UserId);
            res.AddUser(new UserProfile
                { Email = userProfile.Email, FirstName = userProfile.FirstName, LastName = userProfile.LastName });
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet]
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
