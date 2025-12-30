using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
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
        private IMongoClient _mongoClient;

        public LicenseController(IClusterClient client, ILogger<LicenseController> logger, IMongoClient mongoClient)
        {
            _client = client;
            _logger = logger;
            _mongoClient = mongoClient;
        }

        [HttpPost("create")]
        public Task<IActionResult> AddUser([FromBody] User userProfile)
        {

            var res = _client.GetGrain<IUserGrain>(userProfile.UserId);
            res.AddUser(new User
            { Email = userProfile.Email, FirstName = userProfile.FirstName, LastName = userProfile.LastName, UserId = userProfile.UserId});
            _logger.LogInformation("user  with email  {email} created/updated", userProfile.Email);
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpPost("update")]
        public Task<IActionResult> UpdateUser([FromBody] User userProfile)
        {

            var res = _client.GetGrain<IUserGrain>(userProfile.UserId);
            res.Update(new User
                { Email = userProfile.Email, FirstName = userProfile.FirstName, LastName = userProfile.LastName, UserId = userProfile.UserId});
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
               return Ok(profile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Problem("User not found", statusCode: 404);  
            }

        }

        [HttpGet("getAudit")]
        public async Task<IActionResult> GetAudit([FromQuery] string userId)
        {
            try
            {
                var res = _client.GetGrain<IUserGrain>(userId);
                var profile = await res.GetAuditTrail();
                return Ok(profile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Problem("User not found", statusCode: 404);
            }

        }

        [HttpGet("getAllLicenses")]
        public async Task<IActionResult> GetAudit()
        {
            try
            {
                var database = _mongoClient.GetDatabase("TerminalGatewayDb");
                var collection = database.GetCollection<BsonDocument>("GrainsUserGrain");
                //var projection = Builders<BsonDocument>.Projection
                //    .Slice("__values", -1)
                //    .Exclude("_id")
                //    .Include("_doc.Log.__values");

                var pipeline = new BsonDocument[]
                {
                    //new BsonDocument("$project", new BsonDocument
                    //{
                    //   // { "_id", 1 }, // Keep the _id field
                    //    { "LastArrayElement", new BsonDocument("$arrayElemAt", new BsonArray { "$_doc.Log.__values", -1 }) },
                    //    { "_id", 0 }
                    //    // The path to your array: "$Level1.Level2.MyArray"
                    //    // The index -1 refers to the last element
                    //})

                    new BsonDocument("$project", new BsonDocument
                    {
                       // { "_id", 1 }, // Keep the _id field
                        { "LastArrayElement", new BsonDocument("$arrayElemAt", new BsonArray { "$_doc.Log.__values", -1 }) },
                        { "_id", 0 }
                        // The path to your array: "$Level1.Level2.MyArray"
                        // The index -1 refers to the last element
                    })

                };



                var result = await collection.AggregateAsync<BsonDocument>(pipeline).Result.ToListAsync();
               
                //var filter = Builders<BsonDocument>.Filter.Empty;


                //var resultDocument = await collection.Find(filter).Project(projection).ToListAsync();


                ////foreach (var VARIABLE in resultDocument)
                ////{

                ////    var val = VARIABLE.Elements.LastOrDefault().Value;
                ////}

                return Ok(result.ToJson());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Problem("User not found", statusCode: 404);
            }

        }
    }

    public class GrainStateDocument<T>
    {
        public string Id { get; set; }

        [BsonElement("_values")]
        public T Doc { get; set; } = default!;

        [BsonConstructor]
        public GrainStateDocument(string id, T doc)
        {
            Id = id;
            Doc = doc;
           
        }
    }

}
