using MyMongoApp.Enums;
using System.Collections.Generic;

namespace MyMongoApp.Dtos
{
    public class BusinessDto
    {
        public List<BusinessEntryDto> Businesses { get; set; }

        public ContactDetailsDto? Contact { get; set; }
    }

    public class BusinessEntryDto
    {
        public BusinessType Type { get; set; }

        public string NameOrNumber { get; set; }

        public AddressDto? Address { get; set; }  // ← NEW
    }

    public class ContactDetailsDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? Alias { get; set; }

        public string Designation { get; set; }

        public ContactMode Mode { get; set; }

        public List<PhoneEntryDto> PhoneNumbers { get; set; }

        public List<EmailEntryDto> Emails { get; set; }

        public string? Notes { get; set; }
    }

    public class PhoneEntryDto
    {
        public string Value { get; set; }

        public ContactType Type { get; set; }
    }

    public class EmailEntryDto
    {
        public string Value { get; set; }

        public ContactType Type { get; set; }
    }

    public class AddressDto
    {
        public string? Building { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
    }

}
