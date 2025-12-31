using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using Terminal.Gateway.Grains;
using Terminal.Gateway.MongoUtils;
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
        public async Task<IActionResult> GetAudit(int page, int size)
        {
            try
            {
                var database = _mongoClient.GetDatabase("TerminalGatewayDb");
                var collection = database.GetCollection<BsonDocument>("GrainsUserGrain");
                ////var projection = Builders<BsonDocument>.Projection
                ////    .Slice("__values", -1)
                ////    .Exclude("_id")
                ////    .Include("_doc.Log.__values");

                //var projectStage =
                //    new BsonDocument("$project", new BsonDocument
                //    {
                //        // { "_id", 1 }, // Keep the _id field
                //        {
                //            "LastArrayElement",
                //            new BsonDocument("$arrayElemAt", new BsonArray { "$_doc.Log.__values", -1 })
                //        },
                //        { "_id", 0 }
                //        // The path to your array: "$Level1.Level2.MyArray"
                //        // The index -1 refers to the last element
                //    });

                //var skipStage = new BsonDocument("$skip", 1);
                //var limitStage = new BsonDocument("$limit", 2);

                //// 3. Define the sub-pipeline for the 'data' facet using BsonDocument array
                //var dataFacetPipeline = new BsonArray
                //{
                //    projectStage,
                //    skipStage,
                //    limitStage
                //};

                //// 4. Define the sub-pipeline for the 'metadata' (count) facet
                //var metadataFacetPipeline = new BsonArray
                //{
                //    new BsonDocument("$count", "total")
                //};

                //// 5. Combine sub-pipelines into the main $facet stage
                //var facetStage = new BsonDocument("$facet", new BsonDocument
                //{
                //    { "metadata", metadataFacetPipeline },
                //    { "data", dataFacetPipeline }
                //});

                //// 6. Create the full pipeline and execute
                //var pipeline = new[]
                //{
                //    facetStage
                //};

                //var pipelineDefinition = PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline);

                //var results = await collection.Aggregate(pipelineDefinition).FirstOrDefaultAsync();
                //var json = results.ToJson();
                var json = await collection.AggregateByPage(
                    new BsonDocument("$project", new BsonDocument
                    {
                        // { "_id", 1 }, // Keep the _id field
                        {
                            "User",
                            new BsonDocument("$arrayElemAt", new BsonArray { "$_doc.Log.__values", -1 })
                        },
                        {"_type", 0},
                        { "_id", 1 }
                        // The path to your array: "$Level1.Level2.MyArray"
                        // The index -1 refers to the last element
                    }),
                    page: page,
                    pageSize: size);


                return Ok(json);
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
