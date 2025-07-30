using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;
using System;
using System.Collections.Generic;

namespace MyMongoApp.Models
{
    public class Quote
    {
        /// <summary>
        /// unique quote id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// business associated with the quote
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessId { get; set; } = string.Empty;

        /// <summary>
        /// date of the quote
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// just a placeholder field, no significance
        /// </summary>
        public string FirstResponseTeam { get; set; } = string.Empty;

        /// <summary>
        /// services in the quote
        /// </summary>
        public List<ServiceLine>? Services { get; set; }

        /// <summary>
        /// discount field
        /// </summary>
        public double DiscountPercentage { get; set; }

        /// <summary>
        /// vat percentage field
        /// </summary>
        public int VatPercentage { get; set; }

        /// <summary>
        /// sub total
        /// </summary>
        public double Subtotal { get; set; }

        /// <summary>
        /// vat amount calculated
        /// </summary>
        public double VatAmount { get; set; }

        /// <summary>
        /// total quote
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// an auto-increment field
        /// </summary>
        public string QSID { get; set; } = "-1";
    }

    public class ServiceLine
    {
        /// <summary>
        /// service type
        /// </summary>
        public QuoteServiceType Service { get; set; } = QuoteServiceType.Unknown;

        /// <summary>
        /// description if any
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Amount of the service
        /// </summary>
        public double Amount { get; set; }
    }

}
