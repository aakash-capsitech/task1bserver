using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;

namespace MyMongoApp.Models
{
    public class AuditLog
    {
        /// <summary>
        /// log id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// currently applicable only for login rules, further modification in future
        /// </summary>
        public AuditLogEntity EntityType { get; set; } = AuditLogEntity.Unknown;

        /// <summary>
        /// login rule id
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// login rule id, user email
        /// </summary>
        public IdNameModel? Target { get; set; }

        /// <summary>
        /// action (created, updated)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        public CreatedBy? PerformedBy { get; set; }

        /// <summary>
        /// description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class IdNameModel
    {
        /// <summary>
        /// login rule id
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// name of the user
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    public class CreatedBy
    {
        /// <summary>
        /// creator id
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// creator name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// created timing
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
