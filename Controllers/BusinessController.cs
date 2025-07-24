using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MyMongoApp.Data;
using MyMongoApp.Dtos;
using MyMongoApp.Enums;
using MyMongoApp.Models;


namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessesController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public BusinessesController(MongoDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Create Business
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateBusiness([FromBody] BusinessDto dto)
        {
            var contact = new ContactDetails
            {
                FirstName = dto.Contact.FirstName,
                LastName = dto.Contact.LastName,
                Alias = dto.Contact.Alias,
                Designation = dto.Contact.Designation,
                Mode = Enum.TryParse<ContactMode>(dto.Contact.Mode, true, out var mode) ? mode : ContactMode.Unknown,
                Notes = dto.Contact.Notes,
                PhoneNumbers = dto.Contact.PhoneNumbers
                    .Where(p => p != null)
                    .Select(p => new PhoneEntry
                    {
                        Value = p.Value,
                        Type = Enum.TryParse<ContactType>(p.Type, true, out var pType) ? pType : ContactType.Unknown
                    }).ToList(),
                Emails = dto.Contact.Emails
                    .Where(e => e != null)
                    .Select(e => new EmailEntry
                    {
                        Value = e.Value,
                        Type = Enum.TryParse<ContactType>(e.Type, true, out var eType) ? eType : ContactType.Unknown
                    }).ToList()
            };

            await _context.Contacts.InsertOneAsync(contact);
            var contactId = contact.Id;

            var businessEntities = dto.Businesses
                .Where(b => b != null)
                .Select(b => new Business
                {
                    BusinessE = new BusinessEntry
                    {
                        Type = Enum.TryParse<BusinessType>(b.Type, true, out var bt) ? bt : BusinessType.Unknown,
                        NameOrNumber = b.NameOrNumber,
                        Address = b.Address == null ? null : new Address
                        {
                            Building = b.Address.Building,
                            Street = b.Address.Street,
                            City = b.Address.City,
                            County = b.Address.County,
                            Postcode = b.Address.Postcode,
                            Country = b.Address.Country
                        }
                    },
                    ContactId = contactId
                });

            await _context.Businesses.InsertManyAsync(businessEntities);


            return Ok();
        }


        /// <summary>
        /// Get businesses with filter
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? type = null)
        {
            var filterBuilder = Builders<Business>.Filter;
            var filters = new List<FilterDefinition<Business>>();

            if (!string.IsNullOrEmpty(search))
            {
                var regex = new BsonRegularExpression(search, "i");
                filters.Add(filterBuilder.Regex("BusinessE.NameOrNumber", regex));
            }

            if (!string.IsNullOrEmpty(type) &&
                Enum.TryParse(type, true, out BusinessType parsedType))
            {
                filters.Add(filterBuilder.Eq("BusinessE.Type", parsedType));
            }

            var finalFilter = filters.Any()
                ? filterBuilder.And(filters)
                : FilterDefinition<Business>.Empty;

            var total = await _context.Businesses.CountDocumentsAsync(finalFilter);

            var businesses = await _context.Businesses
                .Find(finalFilter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var contactIds = businesses
                .Where(b => b.ContactId != null)
                .Select(b => b.ContactId)
                .Distinct()
                .ToList();

            var contacts = await _context.Contacts
                .Find(Builders<ContactDetails>.Filter.In(c => c.Id, contactIds))
                .ToListAsync();

            var contactMap = contacts.ToDictionary(c => c.Id, c => c);

            var enrichedBusinesses = businesses.Select(b =>
            {
                var contactId = b.ContactId;
                var contact = contactId != null && contactMap.ContainsKey(contactId)
                    ? contactMap[contactId]
                    : null;

                return new
                {
                    id = b.Id,
                    businessE = b.BusinessE,
                    contact
                };
            });

            return Ok(new { total, businesses = enrichedBusinesses });
        }
    }
}
