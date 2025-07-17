using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MyMongoApp.Data;
using MyMongoApp.Dtos;
using MyMongoApp.Enums;
using MyMongoApp.Models;

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


        [HttpGet]
        public async Task<IActionResult> GetAll(
    int page = 1,
    int pageSize = 10,
    string? search = null)
        {
            // Fetch all login rules
            var rules = await _context.LoginRules.Find(_ => true).ToListAsync();

            // Get distinct user IDs from rules
            var allUserIds = rules.Select(r => r.UserId).Distinct().ToList();

            // Fetch users corresponding to those user IDs
            var users = await _context.Users.Find(u => allUserIds.Contains(u.Id)).ToListAsync();

            // Create a dictionary mapping user ID to email
            var userIdToEmail = users.ToDictionary(u => u.Id, u => u.Email);

            // Project enriched rules with email included
            var enrichedRules = rules.Select(rule => new LoginRuleDto
            {
                Id = rule.Id,
                Restriction = rule.Restriction.ToString(),
                FromDate = rule.FromDate?.ToString("o"),
                ToDate = rule.ToDate?.ToString("o"),
                UserEmail = userIdToEmail.GetValueOrDefault(rule.UserId, "Unknown")
            });

            // Filter by email search if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                enrichedRules = enrichedRules
                    .Where(r => r.UserEmail.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Materialize to list
            var list = enrichedRules.ToList();
            var total = list.Count;

            // Apply pagination
            var paginated = list
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
            if (string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest("At least one user must be selected.");

            // ✅ Fetch user info for logging
            var user = await _context.Users.Find(u => u.Id == dto.UserId).FirstOrDefaultAsync();
            if (user == null)
                return NotFound("User not found.");

            var rule = new LoginRule
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = dto.UserId,
                Restriction = dto.Restriction,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate
            };

            await _context.LoginRules.InsertOneAsync(rule);

            // ✅ Insert enriched audit log
            var log = new AuditLog
            {
                EntityType = AuditLogEntity.LoginRule,
                EntityId = rule.Id,
                Target = new IdNameModel { Id = rule.Id, Name = user.Email }, // or user.Name if you prefer
                Action = "Created",
                PerformedBy = new CreatedBy
                {
                    Id = user.Id,
                    Name = user.Name, // or user.Email
                    Timestamp = DateTime.UtcNow
                },
                Description = $"Created login rule for user {user.Email} with restriction {rule.Restriction}",
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.InsertOneAsync(log);

            return Ok(rule);
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
            if (dto.UserId == null)
                return BadRequest("Update must target exactly one user per rule.");

            var update = Builders<LoginRule>.Update
                .Set(r => r.UserId, dto.UserId)
                .Set(r => r.Restriction, dto.Restriction)
                .Set(r => r.FromDate, dto.FromDate)
                .Set(r => r.ToDate, dto.ToDate);

            var result = await _context.LoginRules.UpdateOneAsync(r => r.Id == id, update);

            if (result.MatchedCount == 0)
                return NotFound();

            await LogAudit("Updated", id, $"Updated login rule for user {dto.UserId} with restriction {dto.Restriction}");

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
