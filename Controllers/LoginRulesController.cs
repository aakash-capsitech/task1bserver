using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class LoginRulesController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public LoginRulesController(MongoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a paginated list of login rules, enriched with user email information,  and optionally filtered by
        /// a search term.
        /// </summary>
        /// <remarks>This method fetches all login rules from the database, enriches them with the
        /// corresponding  user email addresses, and applies optional filtering and pagination. The search term, if
        /// provided,  is used to filter login rules based on the associated user email.</remarks>
        /// <param name="page">The page number to retrieve. Must be greater than or equal to 1. Defaults to 1.</param>
        /// <param name="pageSize">The number of items per page. Must be greater than or equal to 1. Defaults to 10.</param>
        /// <param name="search">An optional search term used to filter login rules by user email.  If null or empty, no filtering is
        /// applied.</param>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object with the total count of login rules  and the
        /// paginated list of enriched login rules.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? search = null)
            {
            try
            {
                var rules = await _context.LoginRules.Find(_ => true).ToListAsync();

                var allUserIds = rules.Select(r => r.UserId).Distinct().ToList();

                var users = await _context.Users.Find(u => allUserIds.Contains(u.Id)).ToListAsync();

                var userIdToEmail = users.ToDictionary(u => u.Id, u => u.Email);

                var enrichedRules = rules.Select(rule => new LoginRuleDto
                {
                    Id = rule.Id,
                    Restriction = rule.Restriction.ToString(),
                    FromDate = rule.FromDate?.ToString("o"),
                    ToDate = rule.ToDate?.ToString("o"),
                    UserEmail = userIdToEmail.GetValueOrDefault(rule.UserId, "Unknown")
                });

                if (!string.IsNullOrWhiteSpace(search))
                {
                    enrichedRules = enrichedRules
                        .Where(r => r.UserEmail.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                var list = enrichedRules.ToList();
                var total = list.Count;

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
                catch
            {
                return BadRequest("something went wrong");
            }
            }

        /// <summary>
        /// Create Login Rules
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLoginRuleDto dto)
        {
           try
            {
                if (string.IsNullOrWhiteSpace(dto.UserId))
                    return BadRequest("At least one user must be selected.");

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

                var log = new AuditLog
                {
                    EntityType = AuditLogEntity.LoginRule,
                    EntityId = rule.Id,
                    Target = new IdNameModel { Id = rule.Id, Name = user.Email },
                    Action = "Created",
                    PerformedBy = new CreatedBy
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Timestamp = DateTime.UtcNow
                    },
                    Description = $"Created login rule for user {user.Email} with restriction {rule.Restriction}",
                    Timestamp = DateTime.UtcNow
                };

                await _context.AuditLogs.InsertOneAsync(log);

                return Ok(rule);
            }
            catch
            {
                return BadRequest("something went wrong");
            }
        }


        /// <summary>
        /// Update Login Rule
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateLoginRuleDto dto)
        {
            try
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
            catch
            {
                return BadRequest("something went wrong");
            }
        }

        /// <summary>
        /// Update Login Rule
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _context.LoginRules.DeleteOneAsync(r => r.Id == id);
                if (result.DeletedCount == 0)
                    return NotFound();

                await LogAudit("Deleted", id, $"Deleted login rule with ID {id}");

                return NoContent();
            }
            catch
            {
                return BadRequest("something went wrong");
            }
        }

        /// <summary>
        /// Get Audit Log History For Login Rules
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(string id)
        {
            try
            {
                var logs = await _context.AuditLogs
                .Find(log => log.EntityId == id)
                .SortByDescending(log => log.Timestamp)
                .ToListAsync();

                return Ok(logs);
            }
            catch
            {
                return BadRequest("something went wrong");
            }
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
            try
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
            catch
            {
                
            }
        }
    }
}
