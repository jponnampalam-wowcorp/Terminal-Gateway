using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Runtime;

namespace Terminal.Gateway.Grains
{
    [StorageProvider(ProviderName = "UserStorage")]
    [LogConsistencyProvider(ProviderName = "LogStorage")]
    public class UserGrain : JournaledGrain<UserProfileState>, IUserGrain
    {

        private readonly IPersistentState<UserProfile> _profile;

        private readonly ILogger<UserGrain> _logger;

        public UserGrain([PersistentState("profile", "UserStorage")] IPersistentState<UserProfile> profile, ILogger<UserGrain> logger)
        {
            _profile= profile;
            _logger= logger;
        }
        public async Task<UserProfile?>? GetUserProfile()
        {

            //var latestVersion = Version;
            //var latest = await RetrieveConfirmedEvents(latestVersion-1, latestVersion);
            return new UserProfile
            {
                Action = State.Action,
                Email = State.Email,
                FirstName = State.FirstName,
                LastName = State.LastName,
                UpdateDateTime = State.UpdateDateTime,
                Version = Version
            };

        }

        public async Task AddUser(User user)
        {
            
            RaiseEvent(new UserProfileEvent(user, ActionType.Create));
            //_profile.State = profile;
            //_profile.State.UpdateDateTime = DateTime.UtcNow;
            //await _profile.WriteStateAsync();
            //IUserAuditGrain auditGrain = GrainFactory.GetGrain<IUserAuditGrain>(Guid.Empty);
            //await auditGrain.AddAuditEntry(_profile.State, _profile.RecordExists? ActionType.Update: ActionType.Create, this.GetPrimaryKeyString());
            //_logger.LogInformation("Grain with {GetPrimaryKeyString} created", this.GetPrimaryKeyString());
            await ConfirmEvents();
        }

        

        public async Task Update(User user)
        {
            // _profile.State = profile;
            //_profile.State.UpdateDateTime = DateTime.UtcNow;
            //IUserAuditGrain auditGrain = GrainFactory.GetGrain<IUserAuditGrain>(Guid.Empty);
            //await auditGrain.AddAuditEntry(_profile.State, ActionType.Update, this.GetPrimaryKeyString());
            //await _profile.WriteStateAsync();
            //_logger.LogInformation("Grain with {GetPrimaryKeyString} updated", this.GetPrimaryKeyString());
            RaiseEvent(new UserProfileEvent(user, ActionType.Update));
            await ConfirmEvents();
        }

        public async Task<IReadOnlyList<UserProfile>> GetAuditTrail()
        {

            var trail = await RetrieveConfirmedEvents(0, Version);
            List<UserProfile> userProfiles = new List<UserProfile>();
            for(int i = 0; i < trail.Count; i++)
            {
                var item = (UserProfileEvent)trail[i];
            
                userProfiles.Add(new UserProfile
                {
                    Action = item.Action,
                    Email = item.Email,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    UpdateDateTime = item.UpdateDateTime,
                    Version = i+1,
                    UserId = item.UserId
                });
            }

            return userProfiles.OrderByDescending(p => p.Version).ToList();
        }
    }
}
