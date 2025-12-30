using Orleans;

namespace Terminal.Gateway.Grains
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<UserProfile>? GetUserProfile();

        Task AddUser(User profile);

        Task Update(User profile);

        Task<IReadOnlyList<UserProfile>> GetAuditTrail();

    }
}
