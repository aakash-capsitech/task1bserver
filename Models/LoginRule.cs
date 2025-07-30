using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;

namespace MyMongoApp.Models
{
    public class LoginRule
    {
        /// <summary>
        /// unique identifier
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// user associated with the login rule
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// type of restriction
        /// </summary>
        public LoginRulesRestriction Restriction { get; set; } = LoginRulesRestriction.Unknown;

        /// <summary>
        /// denied from this date
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// denied upto this date
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}
