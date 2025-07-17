using System;
using System.Collections.Generic;
using MyMongoApp.Enums;

namespace MyMongoApp.Dtos
{
    public class QuoteDto
    {
        public string? BusinessId { get; set; }

        public DateTime Date { get; set; }

        public string FirstResponseTeam { get; set; } = string.Empty;

        public List<ServiceLineDto>? Services { get; set; }

        public double DiscountPercentage { get; set; }

        public int VatPercentage { get; set; }

        public double Subtotal { get; set; }

        public double VatAmount { get; set; }

        public double Total { get; set; }
    }

    public class ServiceLineDto
    {
        public QuoteServiceType Service { get; set; }  // changed from string to enum

        public string Description { get; set; } = string.Empty;

        public double Amount { get; set; }
    }

}
