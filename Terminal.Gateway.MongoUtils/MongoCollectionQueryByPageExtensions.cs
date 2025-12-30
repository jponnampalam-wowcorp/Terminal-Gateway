using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Terminal.Gateway.MongoUtils
{
    public static class MongoCollectionQueryByPageExtensions
    {
        public static async Task<(int totalPages, IReadOnlyList<TDocument> data)> AggregateByPage<TDocument>(
            this IMongoCollection<TDocument> collection,
            FilterDefinition<TDocument> filterDefinition,
            SortDefinition<TDocument> sortDefinition,
            int page,
            int pageSize)
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
                {
                    PipelineStageDefinitionBuilder.Count<TDocument>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(new[]
                {
                    PipelineStageDefinitionBuilder.Sort(sortDefinition),
                    PipelineStageDefinitionBuilder.Skip<TDocument>((page - 1) * pageSize),
                    PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize),
                }));

            var pipeline = new[]
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
            ProjectionDefinition<BsonDocument> projectionDefinition = pipeline[0];

            var result = await collection.AggregateAsync<BsonDocument>(pipeline).Result.ToListAsync();
            

            var aggregation = await collection.Aggregate()
                .Match(filterDefinition)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count;

            var totalPages = (int)Math.Ceiling((double)count / pageSize);

            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();

            return (totalPages, data);
        }
    }
}
