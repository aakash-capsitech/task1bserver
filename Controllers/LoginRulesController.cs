using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyMongoApp.Data;
using MyMongoApp.Models;
using MyMongoApp.Dtos;

namespace MyMongoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginRulesController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public LoginRulesController(MongoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all Login Rules
        /// </summary>
        /// <returns></returns>
        // [HttpGet]
        // public async Task<IActionResult> GetAll()
        // {
        //     var rules = await _context.LoginRules.Find(_ => true).ToListAsync();

        //     var allUserIds = rules.SelectMany(r => r.UserIds).Distinct().ToList();
        //     var users = await _context.Users.Find(u => allUserIds.Contains(u.Id)).ToListAsync();

        //     var userIdToEmail = users.ToDictionary(u => u.Id, u => u.Email);

        //     var enrichedRules = rules.Select(rule => new
        //     {
        //         id = rule.Id,
        //         restriction = rule.Restriction.ToString(),
        //         fromDate = rule.FromDate,
        //         toDate = rule.ToDate,
        //         userEmails = rule.UserIds.Select(id => userIdToEmail.GetValueOrDefault(id, "Unknown")).ToList()
        //     });

        //     return Ok(enrichedRules);
        // }


     [HttpGet]
public async Task<IActionResult> GetAll(
    int page = 1,
    int pageSize = 10,
    string? search = null)
{
    var rules = await _context.LoginRules.Find(_ => true).ToListAsync();

    var allUserIds = rules.SelectMany(r => r.UserIds).Distinct().ToList();
    var users = await _context.Users.Find(u => allUserIds.Contains(u.Id)).ToListAsync();
    var userIdToEmail = users.ToDictionary(u => u.Id, u => u.Email);

    var enrichedRules = rules.Select(rule => new LoginRuleDto
    {
        Id = rule.Id,
        Restriction = rule.Restriction.ToString(),
        FromDate = rule.FromDate?.ToString("o"),
        ToDate = rule.ToDate?.ToString("o"),
        UserEmails = rule.UserIds
            .Select(id => userIdToEmail.GetValueOrDefault(id, "Unknown"))
            .ToList()
    });

    // 🔍 Search filtering (on email)
    if (!string.IsNullOrWhiteSpace(search))
    {
        enrichedRules = enrichedRules
            .Where(r => r.UserEmails.Any(email =>
                email.Contains(search, StringComparison.OrdinalIgnoreCase)));
    }

    // 📄 Pagination
    var total = enrichedRules.Count();
    var paginated = enrichedRules
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return Ok(new
    {
        total,
        rules = paginated
    });
}



        /// <summary>
        /// Create Login Rules
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLoginRuleDto dto)
        {
            if (dto.UserIds == null || dto.UserIds.Count == 0)
                return BadRequest("At least one user must be selected.");

            var rules = dto.UserIds.Select(userId => new LoginRule
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                UserIds = new List<string> { userId },
                Restriction = dto.Restriction,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate
            }).ToList();

            await _context.LoginRules.InsertManyAsync(rules);

            var logs = rules.Select(rule => new AuditLog
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Action = "Created",
                EntityId = rule.Id,
                Description = $"Created login rule for user {rule.UserIds.First()} with restriction {rule.Restriction}",
                Timestamp = DateTime.UtcNow
            }).ToList();

            await _context.AuditLogs.InsertManyAsync(logs);

            return Ok(rules);
        }

        /// <summary>
        /// Update Login Rule
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateLoginRuleDto dto)
        {
            if (dto.UserIds == null || dto.UserIds.Count != 1)
                return BadRequest("Update must target exactly one user per rule.");

            var update = Builders<LoginRule>.Update
                .Set(r => r.UserIds, dto.UserIds)
                .Set(r => r.Restriction, dto.Restriction)
                .Set(r => r.FromDate, dto.FromDate)
                .Set(r => r.ToDate, dto.ToDate);

            var result = await _context.LoginRules.UpdateOneAsync(r => r.Id == id, update);

            if (result.MatchedCount == 0)
                return NotFound();

            await LogAudit("Updated", id, $"Updated login rule for user {dto.UserIds.First()} with restriction {dto.Restriction}");

            return NoContent();
        }

        /// <summary>
        /// Update Login Rule
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _context.LoginRules.DeleteOneAsync(r => r.Id == id);
            if (result.DeletedCount == 0)
                return NotFound();

            await LogAudit("Deleted", id, $"Deleted login rule with ID {id}");

            return NoContent();
        }

        /// <summary>
        /// Get Audit Log History For Login Rules
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(string id)
        {
            var logs = await _context.AuditLogs
                .Find(log => log.EntityId == id)
                .SortByDescending(log => log.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }

        /// <summary>
        /// Audit Log
        /// </summary>
        /// <param name="action"></param>
        /// <param name="entityId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        private async Task LogAudit(string action, string entityId, string description)
        {
            var log = new AuditLog
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(), 
                Action = action,
                EntityId = entityId,
                Description = description,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.InsertOneAsync(log);
        }
    }
}
