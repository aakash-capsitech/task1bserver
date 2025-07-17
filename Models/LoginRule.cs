using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;

namespace MyMongoApp.Models
{
    public class LoginRule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public LoginRulesRestriction Restriction { get; set; } = LoginRulesRestriction.Unknown;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
