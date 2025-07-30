using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;

namespace MyMongoApp.Models
{
    public class User
    {
        /// <summary>
        /// unique Id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// full name of the user
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address associated with the user.
        /// </summary>
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// logins field is just for the newly created users to chnage their password, until the login count increases they can input 12345 as their current passwod, after that they will have to put their original password to change the password
        /// </summary>
        [BsonElement("logins")]
        public int Logins { get; set; } = 0;

        /// <summary>
        /// the role assigned to the user.
        /// </summary>
        [BsonElement("role")]
        public UserRole Role { get; set; } = UserRole.Unknown;

        /// <summary>
        /// phone number associated with the entity.
        /// </summary>
        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// nationality of the individual.
        /// </summary>
        [BsonElement("nationality")]
        public string Nationality { get; set; } = string.Empty;

        /// <summary>
        /// address associated with the entity.
        /// </summary>
        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// config roles associated with the entity.
        /// </summary>
        [BsonElement("configRoles")]
        [BsonRepresentation(BsonType.String)]
        public List<UserConfigRole> ConfigRoles { get; set; } = new();

        /// <summary>
        /// hashed password field
        /// </summary>
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = "12345";

        /// <summary>
        /// active status of the user
        /// </summary>
        [BsonElement("status")]
        public UserStatus status { get; set; } = UserStatus.Unknown;
    }
}
