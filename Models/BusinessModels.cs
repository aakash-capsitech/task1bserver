using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;
using System.Collections.Generic;

namespace MyMongoApp.Models
{
    public class Business
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public List<BusinessEntry> Businesses { get; set; }

        public ContactDetails? Contact { get; set; }
    }

    public class BusinessEntry
    {
        public BusinessType Type { get; set; }

        public string NameOrNumber { get; set; }

        public Address? Address { get; set; }  // ← NEW
    }

    public class ContactDetails
    {
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

    public class Address
    {
        public string? Building { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
    }

}
