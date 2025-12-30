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

        [Id(3)]
        public DateTime UpdateDateTime { get; set; } = DateTime.MinValue;

        [Id(4)] public ActionType Action { get; set; } = ActionType.Create;

        [Id(5)] public int Version { get; set; }

        [Id(6)] public string UserId { get; set; }
    }

    public enum ProfileStatus
    {
        Active,
        Inactive,
        Suspended
    }

    public enum ActionType
    {
        Create,
        Update,
        Delete
    }
}
