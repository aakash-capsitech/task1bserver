using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public AuditLogEntity EntityType { get; set; } = AuditLogEntity.Unknown;
    public string EntityId { get; set; } = string.Empty;  // the LoginRule ID
    public string Action { get; set; } = string.Empty;    // Created, Updated, Deleted
    public string PerformedBy { get; set; } = string.Empty; // Optional: User who did it
    public string Description { get; set; } = string.Empty; // What changed
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}






// using MongoDB.Bson;
// using MongoDB.Bson.Serialization.Attributes;
// using MyMongoApp.Enums;

// public class AuditLog
// {
//     public string Id { get; set; }
//     public AuditLogEntity EntityType { get; set; } = AuditLogEntity.Unknown;
//      public IdNameModel? Target{} // the LoginRule ID
//     public string Action { get; set; } = string.Empty;    // Created, Updated, Deleted
//     public string PerformedBy { get; set; } = string.Empty; // Optional: User who did it
//     public string Description { get; set; } = string.Empty; // What changed
//     public CreatedBy CreatedBy { get; set; }
// }

// public class IdNameModel
// {
//     [BsonId]
//     [BsonRepresentation(Object.ObjectId)]
//     public string Id { get; set; }
//     public string Name { get; set; } = string.Empty;
// }

// public class CreatedBy
// {
//     [BsonId]
//     [BsonRepresentation(Object.ObjectId)]
//     public string Id { get; set; }
//     /// <summary>
//     /// 
//     /// </summary>
//     public string Name { get; set; } = string.Empty; //User name
//     public DateTime Timestamp { get; set; } = DateTime.UtcNow;
// }