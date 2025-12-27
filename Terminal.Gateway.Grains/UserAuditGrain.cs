using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Terminal.Gateway.MongoUtils;

namespace Terminal.Gateway.Grains
{
    [StatelessWorker]
    public class UserAuditGrain: Grain, IUserAuditGrain
    {
        private IMongoClient _mongoClient;
        private ILogger<UserAuditGrain> _logger;
        public UserAuditGrain(IMongoClient mongoClient, ILogger<UserAuditGrain> logger)
        {
            _mongoClient = mongoClient;
            _logger = logger;
        }
        public Task AddAuditEntry(UserProfile entry, ActionType action, string grainKey)
        {
            UserProfileAudit auditProfile = new UserProfileAudit
            {
                FirstName = entry.FirstName,
                LastName = entry.LastName,
                Email = entry.Email,
                UpdateDateTime = entry.UpdateDateTime,
                Status = entry.Status,
                ActionType = action,
                GrainKey = grainKey
            };

            _mongoClient.GetDatabase("TerminalGatewayDb")
                .GetCollection<UserProfile>("UserAudits")
                .InsertOne(auditProfile);
            return Task.CompletedTask;
        }

        public async Task<(int,IReadOnlyList<UserProfileAudit>)> GetAuditEntryByGrainKey(string grainKey)
        {

            var filter = Builders<UserProfileAudit>.Filter.Eq(x => x.GrainKey, grainKey);
            var sortDefinition = Builders<UserProfileAudit>.Sort.Descending(u => u.UpdateDateTime);
            var collection = _mongoClient.GetDatabase("TerminalGatewayDb")
                .GetCollection<UserProfileAudit>("UserAudits");

            var data = await collection.AggregateByPage(filter, sortDefinition, 1, 10);

            return data;

        }

        public Task<UserAuditGrain> GetAllAuditEntryForAllGrains()
        {
            throw new NotImplementedException();
        }
    }
}
