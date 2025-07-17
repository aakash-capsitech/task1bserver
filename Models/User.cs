// using MongoDB.Bson;
// using MongoDB.Bson.Serialization.Attributes;
// using MyMongoApp.Enums;

// namespace MyMongoApp.Models
// {
//     public class User
//     {
//         [BsonId]
//         [BsonRepresentation(BsonType.ObjectId)]
//         public string Id { get; set; }

//         [BsonElement("name")]
//         public string Name { get; set; } = string.Empty;

//         [BsonElement("email")]
//         public string Email { get; set; } = string.Empty;

//         [BsonElement("role")]
//         public string Role { get; set; } = string.Empty;

//         [BsonElement("phone")]
//         public string Phone { get; set; } = string.Empty;

//         [BsonElement("nationality")]
//         public string Nationality { get; set; } = string.Empty;

//         [BsonElement("address")]
//         public string Address { get; set; } = string.Empty;

//         [BsonElement("configRoles")]
//         [BsonRepresentation(BsonType.String)]
//         public List<UserConfigRole> ConfigRoles { get; set; } = new();
//     }
// }






using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;

namespace MyMongoApp.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("logins")]
        public int Logins { get; set; } = 0;

        [BsonElement("role")]
        public UserRole Role { get; set; } = UserRole.Unknown;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("nationality")]
        public string Nationality { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("configRoles")]
        [BsonRepresentation(BsonType.String)]
        public List<UserConfigRole> ConfigRoles { get; set; } = new();

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = "12345";

        [BsonElement("status")]
        public UserStatus status { get; set; } = UserStatus.Unknown;
    }
}
