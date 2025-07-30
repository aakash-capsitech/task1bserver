using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;
using System.Collections.Generic;

namespace MyMongoApp.Models
{

    public class Business
    {
        /// <summary>
        /// unique id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// business details
        /// </summary>
        public BusinessEntry BusinessE { get; set; }

        /// <summary>
        /// reference to contact
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ContactId { get; set; }

        /// <summary>
        /// auto-incrementing, implemented in the business controller
        /// </summary>
        public string BSID { get; set; }

    }


    public class BusinessEntry
    {
        /// <summary>
        /// business type
        /// </summary>
        public BusinessType Type { get; set; }

        /// <summary>
        /// mainly for name
        /// </summary>
        public string NameOrNumber { get; set; }

        /// <summary>
        /// business address
        /// </summary>
        public Address? Address { get; set; }  // ← NEW
    }

    /// <summary>
    /// address details
    /// </summary>
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
