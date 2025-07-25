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

        public BusinessEntry BusinessE { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? ContactId { get; set; }

        public string BSID { get; set; }

    }


    public class BusinessEntry
    {
        public BusinessType Type { get; set; }

        public string NameOrNumber { get; set; }

        public Address? Address { get; set; }  // ← NEW
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
