using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyMongoApp.Data;
using MyMongoApp.Dtos;
using MyMongoApp.Models;

namespace MyMongoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotesController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public QuotesController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromBody] QuoteDto dto)
        {
            //Logger.info(dto);
            var quote = new Quote
            {
                BusinessId = dto.BusinessId,
                Date = dto.Date,
                FirstResponseTeam = dto.FirstResponseTeam,
                Services = dto.Services.Select(s => new ServiceLine
                {
                    Service = s.Service,
                    Description = s.Description,
                    Amount = s.Amount
                }).ToList(),

                DiscountPercentage = dto.DiscountPercentage,
                VatPercentage = dto.VatPercentage,
                Subtotal = dto.Subtotal,
                VatAmount = dto.VatAmount,
                Total = dto.Total
            };

            await _context.Quotes.InsertOneAsync(quote);
            return Ok(quote.Id);
        }



        //[HttpPost]
        //public async Task<IActionResult> CreateQuote([FromBody] JsonElement json)
        //{
        //    try
        //    {
        //        var quote = JsonSerializer.Deserialize<Quote>(json.GetRawText(), new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });

        //        if (quote is null)
        //            return BadRequest("Invalid quote payload.");

        //        await _context.Quotes.InsertOneAsync(quote);
        //        return Ok(quote.Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: log error
        //        return BadRequest(new { error = "Could not parse quote", detail = ex.Message });
        //    }
        //}


        [HttpGet]
        public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? team = null)
        {
        var quoteFilter = Builders<Quote>.Filter.Empty;
        var filters = new List<FilterDefinition<Quote>>();

        if (!string.IsNullOrWhiteSpace(team))
        {
            filters.Add(Builders<Quote>.Filter.Eq(q => q.FirstResponseTeam, team));
        }

        // Apply quote filters
        if (filters.Count > 0)
            quoteFilter = Builders<Quote>.Filter.And(filters);

        // Get matching quotes
        var quotes = await _context.Quotes
            .Find(quoteFilter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        var total = await _context.Quotes.CountDocumentsAsync(quoteFilter);

        // Get all unique businessIds involved in the quotes
        var businessIds = quotes.Select(q => q.BusinessId).Distinct().ToList();

        // Fetch business info
        var businesses = await _context.Businesses
            .Find(b => businessIds.Contains(b.Id))
            .ToListAsync();

        // Optional in-memory search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();

            quotes = quotes.Where(q =>
            {
                var business = businesses.FirstOrDefault(b => b.Id == q.BusinessId);
                var matchesBusiness =
                    business?.Businesses.Any(be => be.NameOrNumber.ToLower().Contains(searchLower)) == true ||
                    (business?.Contact?.FirstName?.ToLower().Contains(searchLower) ?? false) ||
                    (business?.Contact?.LastName?.ToLower().Contains(searchLower) ?? false) ||
                    (business?.Contact?.Emails?.Any(e => e.Value.ToLower().Contains(searchLower)) ?? false) ||
                    (business?.Contact?.PhoneNumbers?.Any(p => p.Value.ToLower().Contains(searchLower)) ?? false);

                return matchesBusiness;
            }).ToList();
        }

        // Return mapped result
        var result = quotes.Select(q =>
        {
            var business = businesses.FirstOrDefault(b => b.Id == q.BusinessId);
            var businessName = business?.Businesses.FirstOrDefault()?.NameOrNumber ?? "(Deleted)";

            return new
            {
                q.Id,
                BusinessName = businessName,
                q.FirstResponseTeam,
                q.Date,
                q.DiscountPercentage,
                q.VatPercentage,
                q.Subtotal,
                q.VatAmount,
                q.Total
            };
        }).ToList();

        return Ok(new
        {
            total,
            quotes = result
        });
        }

    }
}
