using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;
using MyMongoApp.Models;

namespace MyMongoApp.Models
{
    public class ContactDetails
    {
        /// <summary>
        /// unique id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// alias
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// designation of the contact
        /// </summary>
        public string Designation { get; set; } = string.Empty;

        /// <summary>
        /// mode of contact
        /// </summary>
        public ContactMode Mode { get; set; }

        /// <summary>
        /// phone list
        /// </summary>
        public List<PhoneEntry> PhoneNumbers { get; set; } = new();

        /// <summary>
        /// email list
        /// </summary>
        public List<EmailEntry> Emails { get; set; } = new();

        /// <summary>
        /// notes if any
        /// </summary>
        public string? Notes { get; set; }
    }

    public class PhoneEntry
    {
        /// <summary>
        /// phone number
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// type of phone number
        /// </summary>
        public ContactType Type { get; set; }
    }

    public class EmailEntry
    {
        /// <summary>
        /// email
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// type of email
        /// </summary>
        public ContactType Type { get; set; }
    }

}
