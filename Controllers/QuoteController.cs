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

        //[HttpPost("calc")]
        //public IActionResult CalculateQuote([FromBody] dynamic request)
        //{
        //    var services = ((JsonElement)request.services).EnumerateArray();
        //    var discountPercentage = ((JsonElement)request.discountPercentage).GetDecimal();
        //    var vatPercentage = ((JsonElement)request.vatPercentage).GetDecimal();

        //    // Calculate subtotal from services
        //    var subtotal = 0m;
        //    //foreach (var service in services)
        //    //{
        //    //    subtotal += service.GetProperty("amount").GetDecimal();
        //    //}

        //    // Calculate discount amount
        //    var discountAmount = subtotal * (discountPercentage / 100);

        //    // Calculate VAT amount (applied after discount)
        //    var vatAmount = (subtotal - discountAmount) * (vatPercentage / 100);

        //    // Calculate total
        //    var total = subtotal - discountAmount + vatAmount;

        //    var result = new
        //    {
        //        Subtotal = Math.Round(subtotal, 2),
        //        DiscountAmount = Math.Round(discountAmount, 2),
        //        VatAmount = Math.Round(vatAmount, 2),
        //        Total = Math.Round(total, 2)
        //    };

        //    return Ok(result);
        //}


        [HttpPost("calc")]
        public IActionResult CalculateQuote([FromBody] JsonElement request)
        {
            var servicesElement = request.GetProperty("services");
            var discountPercentage = request.GetProperty("discountPercentage").GetDecimal();
            var vatPercentage = request.GetProperty("vatPercentage").GetDecimal();

            decimal subtotal = 0m;

            foreach (var service in servicesElement.EnumerateArray())
            {
                if (service.TryGetProperty("amount", out var amountProperty) &&
                    amountProperty.TryGetDecimal(out var amount))
                {
                    subtotal += amount;
                }
            }

            // Calculate discount amount
            var discountAmount = subtotal * (discountPercentage / 100);

            // Calculate VAT amount (applied after discount)
            var vatAmount = (subtotal - discountAmount) * (vatPercentage / 100);

            // Calculate total
            var total = subtotal - discountAmount + vatAmount;

            var result = new
            {
                Subtotal = Math.Round(subtotal, 2),
                DiscountAmount = Math.Round(discountAmount, 2),
                VatAmount = Math.Round(vatAmount, 2),
                Total = Math.Round(total, 2)
            };

            return Ok(result);
        }




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
