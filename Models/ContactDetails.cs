using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;
using MyMongoApp.Models;

namespace MyMongoApp.Models
{
    public class ContactDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? Alias { get; set; }

        public string Designation { get; set; }

        public ContactMode Mode { get; set; }

        public List<PhoneEntry> PhoneNumbers { get; set; }

        public List<EmailEntry> Emails { get; set; }

        public string? Notes { get; set; }
    }

    public class PhoneEntry
    {
        public string? Value { get; set; }

        public ContactType Type { get; set; }
    }

    public class EmailEntry
    {
        public string? Value { get; set; }

        public ContactType Type { get; set; }
    }

}
