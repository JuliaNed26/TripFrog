using System.Text.Json.Serialization;

namespace TripFrogModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Admin,
    Traveler
}
