using System.Text.Json.Serialization;

namespace Identity.Domain.AggregatesModel.UsersAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    PendingVerification = 4
}
