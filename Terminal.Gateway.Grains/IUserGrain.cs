using Orleans;

namespace Terminal.Gateway.Grains
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<UserProfile>? GetUserProfile();

        Task AddUser(UserProfile profile);

        Task Update(UserProfile profile);
    }
}
