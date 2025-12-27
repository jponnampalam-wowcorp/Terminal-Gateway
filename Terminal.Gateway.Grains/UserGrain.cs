using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace Terminal.Gateway.Grains
{
    [StorageProvider(ProviderName = "UserStorage")]
    public class UserGrain : Grain, IUserGrain
    {

        private readonly IPersistentState<UserProfile> _profile;

        private readonly ILogger<UserGrain> _logger;

        public UserGrain([PersistentState("profile", "UserStorage")] IPersistentState<UserProfile> profile, ILogger<UserGrain> logger)
        {
            _profile= profile;
            _logger= logger;
        }
        public Task<UserProfile>? GetUserProfile()
        {
            
            return !_profile.RecordExists ? throw new ApplicationException($"User Not Found") : Task.FromResult(_profile.State);
        }

        public async Task AddUser(UserProfile profile)
        {
            _profile.State = profile;
            _profile.State.UpdateDateTime = DateTime.UtcNow;
            await _profile.WriteStateAsync();
            IUserAuditGrain auditGrain = GrainFactory.GetGrain<IUserAuditGrain>(Guid.Empty);
            await auditGrain.AddAuditEntry(_profile.State, _profile.RecordExists? ActionType.Update: ActionType.Create, this.GetPrimaryKeyString());
            _logger.LogInformation("Grain with {GetPrimaryKeyString} created", this.GetPrimaryKeyString());
        }

        public async Task Update(UserProfile profile)
        {
            _profile.State = profile;
            _profile.State.UpdateDateTime = DateTime.UtcNow;
            IUserAuditGrain auditGrain = GrainFactory.GetGrain<IUserAuditGrain>(Guid.Empty);
            await auditGrain.AddAuditEntry(_profile.State, ActionType.Update, this.GetPrimaryKeyString());
            await _profile.WriteStateAsync();
            _logger.LogInformation("Grain with {GetPrimaryKeyString} updated", this.GetPrimaryKeyString());
        }
    }
}
