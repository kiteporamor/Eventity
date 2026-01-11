using Eventity.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Eventity.DataAccess.Models.Mongo;

public class UserDb
{
    [BsonId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; }
    public string Email { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public UserRoleEnum Role { get; set; }
}