using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyMongoApp.Enums;
using System;
using System.Collections.Generic;

namespace MyMongoApp.Models
{
    public class Quote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessId { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string FirstResponseTeam { get; set; } = string.Empty;

        public List<ServiceLine>? Services { get; set; }

        public double DiscountPercentage { get; set; }

        public int VatPercentage { get; set; }

        public double Subtotal { get; set; }

        public double VatAmount { get; set; }

        public double Total { get; set; }

        public string QSID { get; set; }
    }

    public class ServiceLine
    {
        public QuoteServiceType Service { get; set; } = QuoteServiceType.Unknown;  // changed from string to enum

        public string Description { get; set; } = string.Empty;

        public double Amount { get; set; }
    }

}
