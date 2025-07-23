using MyMongoApp.Enums;
using System.Collections.Generic;

namespace MyMongoApp.Dtos
{
   public class BusinessDto
    {
        public List<BusinessEntryDto> Businesses { get; set; } = new();
        public ContactDto? Contact { get; set; }
    }

    public class BusinessEntryDto
    {
        public string Type { get; set; } = null!;
        public string NameOrNumber { get; set; } = null!;
        public AddressDto? Address { get; set; }
    }

    public class AddressDto
    {
        public string Building { get; set; } = "";
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string County { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string Country { get; set; } = "";
    }

    public class ContactDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? Alias { get; set; }
        public string Designation { get; set; } = "";
        public string Mode { get; set; } = "";
        public string Notes { get; set; } = "";
        public List<PhoneDto> PhoneNumbers { get; set; } = new();
        public List<EmailDto> Emails { get; set; } = new();
    }

    public class PhoneDto
    {
        public string Value { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public class EmailDto
    {
        public string Value { get; set; } = "";
        public string Type { get; set; } = "";
    }

}
