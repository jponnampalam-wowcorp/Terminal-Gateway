using Orleans;

namespace Terminal.Gateway.Grains
{
    [GenerateSerializer, Immutable]
    public record UserProfile
    {
        [Id(0)]
        public string FirstName { get; set; } = string.Empty;

        [Id(1)]
        public string LastName { get; set; } = string.Empty;

        [Id(2)]
        public string Email { get; set; } = string.Empty;


    }
}
