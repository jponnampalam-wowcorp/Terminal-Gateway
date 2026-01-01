using MongoDB.Bson.Serialization.Attributes;
using Orleans;

namespace Terminal.Gateway.Grains
{
    [GenerateSerializer, Immutable]
    public record User
    {
        [BsonElement("_id")]
        [Id(0)] public string UserId { get; set; }

        [Id(1)] public string FirstName { get; set; } = string.Empty;

        [Id(2)] public string LastName { get; set; } = string.Empty;

        [Id(3)] public string Email { get; set; } = string.Empty;

        [Id(4)] public DateTime UpdateDateTime { get; set; } = DateTime.MinValue;

        [Id(5)] public ActionType Action { get; set; } = ActionType.Create;



    }
}
