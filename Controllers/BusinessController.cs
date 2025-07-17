using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyMongoApp.Data;
using MyMongoApp.Models;
using MyMongoApp.Dtos;
using MyMongoApp.Enums;


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

        [HttpPost]
        public async Task<IActionResult> CreateBusiness([FromBody] BusinessDto dto)
        {
            var business = new Business
            {
                Businesses = dto.Businesses.Select(b => new BusinessEntry
                {
                    Type = b.Type,
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
                }).ToList(),
                Contact = dto.Contact == null ? null : new ContactDetails
                {
                    FirstName = dto.Contact.FirstName,
                    LastName = dto.Contact.LastName,
                    Alias = dto.Contact.Alias,
                    Designation = dto.Contact.Designation,
                    Mode = dto.Contact.Mode,
                    Notes = dto.Contact.Notes,
                    PhoneNumbers = dto.Contact.PhoneNumbers.Select(p => new PhoneEntry
                    {
                        Value = p.Value,
                        Type = p.Type
                    }).ToList(),
                    Emails = dto.Contact.Emails.Select(e => new EmailEntry
                    {
                        Value = e.Value,
                        Type = e.Type
                    }).ToList()
                }
            };

            await _context.Businesses.InsertOneAsync(business);
            return Ok(business.Id);
        }

        // [HttpGet]
        // public async Task<IActionResult> GetAll()
        // {
        //     var result = await _context.Businesses.Find(_ => true).ToListAsync();
        //     return Ok(result);
        // }

        //         [HttpGet]
        // public async Task<IActionResult> GetAll(
        //     int page = 1,
        //     int pageSize = 10,
        //     string? search = null,
        //     string? type = null)
        // {
        //     var filterBuilder = Builders<Business>.Filter;
        //     var filters = new List<FilterDefinition<Business>>();

        //     if (!string.IsNullOrEmpty(search))
        //     {
        //         var textFilter = filterBuilder.Or(
        //             filterBuilder.Regex(b => b.NameOrNumber, new MongoDB.Bson.BsonRegularExpression(search, "i")),
        //             filterBuilder.Regex("contact.firstName", new MongoDB.Bson.BsonRegularExpression(search, "i")),
        //             filterBuilder.Regex("contact.lastName", new MongoDB.Bson.BsonRegularExpression(search, "i")),
        //             filterBuilder.Regex("contact.emails.value", new MongoDB.Bson.BsonRegularExpression(search, "i")),
        //             filterBuilder.Regex("contact.phoneNumbers.value", new MongoDB.Bson.BsonRegularExpression(search, "i"))
        //         );
        //         filters.Add(textFilter);
        //     }

        //     if (!string.IsNullOrEmpty(type))
        //     {
        //         filters.Add(filterBuilder.Eq(b => b.Type, type));
        //     }

        //     var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : FilterDefinition<Business>.Empty;

        //     var total = await _context.Businesses.CountDocumentsAsync(finalFilter);

        //     var businesses = await _context.Businesses
        //         .Find(finalFilter)
        //         .Skip((page - 1) * pageSize)
        //         .Limit(pageSize)
        //         .ToListAsync();

        //     return Ok(new
        //     {
        //         total,
        //         businesses
        //     });
        // }

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
        var regex = new MongoDB.Bson.BsonRegularExpression(search, "i");

        filters.Add(filterBuilder.Or(
            // Search in any business nameOrNumber
            filterBuilder.ElemMatch(b => b.Businesses, entry =>
                entry.NameOrNumber.ToLower().Contains(search.ToLower())),

            // Contact name
            filterBuilder.Regex("contact.firstName", regex),
            filterBuilder.Regex("contact.lastName", regex),

            // Email/phone values
            filterBuilder.Regex("contact.emails.value", regex),
            filterBuilder.Regex("contact.phoneNumbers.value", regex)
        ));
    }

    if (!string.IsNullOrEmpty(type))
    {
        if (Enum.TryParse(type, true, out BusinessType parsedType))
        {
            filters.Add(filterBuilder.ElemMatch(b => b.Businesses, entry =>
                entry.Type == parsedType));
        }
    }

    var finalFilter = filters.Count > 0
        ? filterBuilder.And(filters)
        : FilterDefinition<Business>.Empty;

    var total = await _context.Businesses.CountDocumentsAsync(finalFilter);

    var businesses = await _context.Businesses
        .Find(finalFilter)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

    return Ok(new { total, businesses });
}


    }
}
