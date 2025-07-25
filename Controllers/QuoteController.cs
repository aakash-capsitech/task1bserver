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

        /// <summary>
        /// Create Quote
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromBody] QuoteDto dto)
        {

            var qsid = await GenerateQSID();
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
                Total = dto.Total,
                QSID = qsid
            };

            await _context.Quotes.InsertOneAsync(quote);
            return Ok(quote.Id);
        }


        /// <summary>
        /// Calculate quote data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

            var discountAmount = subtotal * (discountPercentage / 100);

            var vatAmount = (subtotal - discountAmount) * (vatPercentage / 100);

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

        /// <summary>
        /// Get Quotes with filter
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="team"></param>
        /// <returns></returns>
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

            if (filters.Count > 0)
                quoteFilter = Builders<Quote>.Filter.And(filters);

            var quotes = await _context.Quotes
                .Find(quoteFilter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var total = await _context.Quotes.CountDocumentsAsync(quoteFilter);

            var businessIds = quotes
                .Select(q => q.BusinessId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var businesses = await _context.Businesses
                .Find(b => businessIds.Contains(b.Id))
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();

                quotes = quotes.Where(q =>
                {
                    var business = businesses.FirstOrDefault(b => b.Id == q.BusinessId);
                    var nameOrNumber = business?.BusinessE?.NameOrNumber?.ToLower();
                    return nameOrNumber != null && nameOrNumber.Contains(searchLower);
                }).ToList();
            }

            var result = quotes.Select(q =>
            {
                var business = businesses.FirstOrDefault(b => b.Id == q.BusinessId);
                var businessName = business?.BusinessE?.NameOrNumber ?? "(Deleted)";

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
                    q.Total,
                    q.Services,
                    q.QSID
                };
            }).ToList();

            return Ok(new
            {
                total,
                quotes = result
            });
        }

        /// <summary>
        /// Generating a new QSID
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateQSID()
        {
            var lastQuote = await _context.Quotes.Find(Builders<Quote>.Filter.Empty)
                .SortByDescending(q => q.QSID)
                .Limit(1)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastQuote != null && !string.IsNullOrEmpty(lastQuote.QSID))
            {
                var numericPart = lastQuote.QSID.Replace("Q-", "");
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string newQSID = $"Q-{nextNumber:D3}";

            return newQSID;
        }
    }
}
