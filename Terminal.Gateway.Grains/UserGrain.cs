using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace Terminal.Gateway.Grains
{
    [StorageProvider(ProviderName = "UserStorage")]
    public class UserGrain : Grain, IUserGrain
    {

        private readonly IPersistentState<UserProfile> _profile;

        public UserGrain([PersistentState("profile", "UserStorage")] IPersistentState<UserProfile> profile)
        {
            _profile= profile;
        }
        public Task<UserProfile>? GetUserProfile()
        {
            return !_profile.RecordExists ? throw new ApplicationException($"User Not Found") : Task.FromResult(_profile.State);
        }

        public async Task AddUser(UserProfile profile)
        {
            _profile.State = profile;
            await _profile.WriteStateAsync();
        }
    }
}
