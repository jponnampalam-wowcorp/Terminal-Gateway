using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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

                var projections = new BsonDocument[]
                {
                    // Stage 1: Extract the last element of the "items" array
                    // and place it into a new field "lastItem"
                    new BsonDocument("$addFields", new BsonDocument
                    {
                        { "lastItem", new BsonDocument("$arrayElemAt", new BsonArray { "$_doc.Log.__values", -1 }) }
                    }),

                    // Stage 2: Merge the fields of "lastItem" into the root document,
                    // effectively "flattening" its contents.
                    new BsonDocument("$replaceRoot", new BsonDocument
                    {
                        { "newRoot", new BsonDocument("$mergeObjects", new BsonArray { "$$ROOT", "$lastItem" }) }
                    }),

                    // Stage 3: (Optional) Project the final output to exclude the original array 
                    // and the intermediate "lastItem" field, if desired.
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "items", 0 }, // Exclude the original array
                        { "lastItem", 0 }, // Exclude the intermediate field
                        { "_doc", 0},
                        {"_etag",0},
                        {"_grainId", 0},
                        {"__type",0},
                        {"__id",0}

                        // Add other fields you want to explicitly include or exclude
                    })
                };

                //var results = await collection.AggregateAsync<BsonDocument>(pipeline);
                //var list = await results.ToListAsync();
                //var json = list.ToJson();

                var json = await collection.AggregateByPage(
                    projections.ToList(),
                    // new BsonDocument("$project", new BsonDocument
                    //{
                    //    { "_id", 1 },
                    //    {
                    //        "User",
                    //        new BsonDocument("$arrayElemAt", new BsonArray { "$_doc.Log.__values", -1 })
                    //    }

                    //    // The path to your array: "$Level1.Level2.MyArray"
                    //    // The index -1 refers to the last element
                    //}),
                    page: page,
                    pageSize: size);

                //JsonNode node = JsonNode.Parse(json);
                //JsonNode dataNode = node["data"];
                //JsonArray arr = dataNode.AsArray();

                //var users = new List<User>();

                //foreach (var user in arr)
                //{

                //    var obj = user.AsObject();

                //    var newUser =JsonSerializer.Deserialize<User>(obj[1].ToString());
                //    newUser.UserId = obj[0].ToString();
                //    //newUser.FirstName = obj[1]["FirstName"].ToString();
                //    //newUser.LastName = obj[1]["LastName"].ToString();
                //    //newUser.Email = obj[1]["Email"].ToString();
                //    //newUser.UpdateDateTime = DateTime.Parse(obj[1]["UpdateDateTime"].ToString()!);
                //    //newUser.Action = Enum.Parse<ActionType>(obj[1]["Action"].ToString()!);
                //    users.Add(newUser);
                //}
                JsonNode node = JsonNode.Parse(json);
                JsonNode dataNode = node["data"];
                JsonNode count = node["totalCount"];
             //   var wrappedJson = new { Users = dataNode }.ToJson(); // Or manually construct: $"{{\"Users\":{dataNode.ToJsonString()}}}"
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var result = dataNode.Deserialize<List<UserSummary>>(options);

                var totalCount = count.Deserialize<List<TotalCount>>(options);
             //   int docCount = totalCount.FirstOrDefault()?.Total.FirstOrDefault() ?? 0;
             var summary = new UsersSummary { Users = result, TotalCount = totalCount.FirstOrDefault().Total };

             var summaryDeSerialized = JsonSerializer.Serialize(summary);
            
                return Ok(summary);  
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

    public class UsersSummary
    {
        public List<UserSummary> Users { get; set; } 
        public int? TotalCount { get; set; }
    }

    public class UserSummary
    {
        [BsonId] // Maps the C# Id property to the MongoDB _id field
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

             public string FirstName { get; set; } = string.Empty;

             public string LastName { get; set; } = string.Empty;

             public string Email { get; set; } = string.Empty;

             public DateTime UpdateDateTime { get; set; } = DateTime.MinValue;

             public ActionType Action { get; set; } = ActionType.Create;



    }

    public class TotalCount
    {
        public int Total { get; set; }
    }

}
