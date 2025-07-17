// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using MongoDB.Driver;
// using MyMongoApp.Data;
// using MyMongoApp.Models;
// using System.Security.Claims;

// namespace MyMongoApp.Controllers
// {
//     [ApiController]
//     [Route("api/logs")]
//     public class LogsController : ControllerBase
//     {
//         private readonly MongoDbContext _context;

//         public LogsController(MongoDbContext context)
//         {
//             _context = context;
//         }

//         /// <summary>
//         /// Fetch logs (admin gets all, others get their own)
//         /// </summary>
//         [HttpGet]
//         [Authorize]
//         public async Task<IActionResult> GetLogs()
//         {
//             var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//             var role = User.FindFirst(ClaimTypes.Role)?.Value;

//             if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
//                 return Unauthorized();

//             FilterDefinition<LogEntry> filter = role == "admin"
//                 ? Builders<LogEntry>.Filter.Empty
//                 : Builders<LogEntry>.Filter.Eq(l => l.UserId, userId);

//             var logs = await _context.Logs
//                 .Find(filter)
//                 .SortByDescending(l => l.Timestamp)
//                 .Limit(200)
//                 .ToListAsync();

//             return Ok(logs);
//         }
//     }
// }
