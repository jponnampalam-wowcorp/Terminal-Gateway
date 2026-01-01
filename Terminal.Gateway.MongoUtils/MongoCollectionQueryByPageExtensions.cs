using MongoDB.Bson;
using MongoDB.Driver;

namespace Terminal.Gateway.MongoUtils;

public static class MongoCollectionQueryByPageExtensions
{
    public static async Task<string> AggregateByPage(
        this IMongoCollection<BsonDocument> collection,
        List<BsonDocument> projectStages,
        int page,
        int pageSize)
    {
        var skipStage = new BsonDocument("$skip", page);
        var limitStage = new BsonDocument("$limit", pageSize);

        var metadataFacetPipeline = new BsonArray
        {
            new BsonDocument("$count", "total")
        };

        // 3. Define the sub-pipeline for the 'data' facet using BsonDocument array
        var listOfBsonDocuments = new List<BsonDocument>();
        listOfBsonDocuments.AddRange(projectStages);
        listOfBsonDocuments.Add(skipStage);
        listOfBsonDocuments.Add(limitStage);
        var dataFacetPipeline = new BsonArray(listOfBsonDocuments);


        // 5. Combine sub-pipelines into the main $facet stage
        var facetStage = new BsonDocument("$facet", new BsonDocument
        { 
            { "totalCount", metadataFacetPipeline },
            { "data", dataFacetPipeline }
        });

        // 6. Create the full pipeline and execute
        var pipeline = new[]
        {
            facetStage
        };

        var pipelineDefinition = PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline);

        var results = await collection.AggregateAsync(pipelineDefinition);
        var data = await results.FirstOrDefaultAsync();

        return data.ToJson();
    }
}