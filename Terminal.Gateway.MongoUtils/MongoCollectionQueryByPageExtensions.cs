using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Terminal.Gateway.MongoUtils
{
    public static class MongoCollectionQueryByPageExtensions
    {
        public static async Task<string> AggregateByPage(
            this IMongoCollection<BsonDocument> collection,
            BsonDocument projectStage,
            int page,
            int pageSize)
        {
            var skipStage = new BsonDocument("$skip", page);
            var limitStage = new BsonDocument("$limit",pageSize);

            // 3. Define the sub-pipeline for the 'data' facet using BsonDocument array
            var dataFacetPipeline = new BsonArray
            {
                projectStage,
                skipStage,
                limitStage
            };

            // 4. Define the sub-pipeline for the 'metadata' (count) facet
            var metadataFacetPipeline = new BsonArray
            {
                new BsonDocument("$count", "total")
            };

            // 5. Combine sub-pipelines into the main $facet stage
            var facetStage = new BsonDocument("$facet", new BsonDocument
            {
                { "metadata", metadataFacetPipeline },
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
}
