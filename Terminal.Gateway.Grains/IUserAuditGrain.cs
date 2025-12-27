using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Gateway.Grains
{
    public  interface IUserAuditGrain : IGrainWithGuidKey
    {
        Task AddAuditEntry(UserProfile entry, ActionType action, string grainKey);
        Task<(int,IReadOnlyList<UserProfileAudit>)> GetAuditEntryByGrainKey(string grainKey);

        Task<UserAuditGrain> GetAllAuditEntryForAllGrains();
    }
}
