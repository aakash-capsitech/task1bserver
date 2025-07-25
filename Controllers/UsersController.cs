using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using MyMongoApp.Data;
using MyMongoApp.Dtos;
using MyMongoApp.Enums;
using MyMongoApp.Models;

namespace MyMongoApp.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public UsersController(MongoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var existingUser = await _context.Users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest("A user with this email already exists.");
            }

            var parsedRoles = dto.ConfigRoles;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                Phone = dto.Phone,
                Nationality = dto.Nationality,
                Address = dto.Address,
                ConfigRoles = parsedRoles
            };

            await _context.Users.InsertOneAsync(user);
            return Ok(user);
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var updates = new List<UpdateDefinition<User>>();

            if (!string.IsNullOrWhiteSpace(dto.Name))
                updates.Add(Builders<User>.Update.Set(u => u.Name, dto.Name));

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existing = await _context.Users.Find(u => u.Email == dto.Email && u.Id != id).FirstOrDefaultAsync();
                if (existing != null)
                {
                    return BadRequest("A user with this email already exists.");
                }
                updates.Add(Builders<User>.Update.Set(u => u.Email, dto.Email));
            }

            if (dto.Role.HasValue)
                updates.Add(Builders<User>.Update.Set(u => u.Role, dto.Role.Value));

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                updates.Add(Builders<User>.Update.Set(u => u.Phone, dto.Phone));

            if (!string.IsNullOrWhiteSpace(dto.Nationality))
                updates.Add(Builders<User>.Update.Set(u => u.Nationality, dto.Nationality));

            if (!string.IsNullOrWhiteSpace(dto.Address))
                updates.Add(Builders<User>.Update.Set(u => u.Address, dto.Address));

            if (dto.ConfigRoles != null && dto.ConfigRoles.Any())
            {
                updates.Add(Builders<User>.Update.Set(u => u.ConfigRoles, dto.ConfigRoles));
            }

            if (!updates.Any())
                return BadRequest("No valid fields provided for update.");

            var updateDef = Builders<User>.Update.Combine(updates);
            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, updateDef);

            if (result.MatchedCount == 0)
                return NotFound();

            return Ok(new { message = "User updated successfully." });
        }

        /// <summary>
        /// Get user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get Profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        /// <summary>
        /// Get All Users with server-side pagination, search, and filtering
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            string? role = null,
            string? status = null)
        {
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            if (!string.IsNullOrEmpty(search))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(u => u.Name, new BsonRegularExpression(search, "i")),
                    filterBuilder.Regex(u => u.Email, new BsonRegularExpression(search, "i")),
                    filterBuilder.Regex(u => u.Phone, new BsonRegularExpression(search, "i"))
                );
                filters.Add(searchFilter);
            }

            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
            {
                filters.Add(filterBuilder.Eq(u => u.Role, parsedRole));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<UserStatus>(status, true, out var parsedStatus))
            {
                filters.Add(filterBuilder.Eq(u => u.status, parsedStatus));
            }

            var combinedFilter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

            var total = await _context.Users.CountDocumentsAsync(combinedFilter);
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var users = await _context.Users
                .Find(combinedFilter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new
            {
                users,
                totalPages,
                total
            });
        }

        /// <summary>
        /// Restore a soft-deleted user
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> RestoreUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("ID cannot be null or empty.");

            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid user ID format.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.status, UserStatus.Active);

            var result = await _context.Users.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                return NotFound("User not found.");

            return Ok("User restored successfully.");
        }


        /// <summary>
        /// Get Filtered Users
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="role"></param>
        /// <param name="nationality"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            UserRole? role = null,
            string? nationality = null)
        {
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            filters.Add(filterBuilder.Ne(u => u.status, UserStatus.Deleted));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var regex = new BsonRegularExpression(search, "i");
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex(u => u.Name, regex),
                    filterBuilder.Regex(u => u.Email, regex),
                    filterBuilder.Regex(u => u.Phone, regex)
                ));
            }

            if (role.HasValue && role.Value != UserRole.Unknown)
            {
                filters.Add(filterBuilder.Eq(u => u.Role, role.Value));
            }

            if (!string.IsNullOrWhiteSpace(nationality))
            {
                var regex = new BsonRegularExpression(nationality, "i");
                filters.Add(filterBuilder.Regex(u => u.Nationality, regex));
            }

            var finalFilter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;


            var total = await _context.Users.CountDocumentsAsync(finalFilter);
            var users = await _context.Users
                .Find(finalFilter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new { total, users });
        }

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var result = await _context.Users.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "User not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// Soft delete User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found" });


            var update = Builders<User>.Update
                .Set(u => u.status, UserStatus.Deleted);

            await _context.Users.UpdateOneAsync(u => u.Id == id, update);

            return Ok(new { message = "User Deleted successfully." });
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto dto)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return NotFound(new { message = "User not found" });

            bool isFirstTimeUser = user.Logins == 0 && user.PasswordHash == "12345";

            if (!isFirstTimeUser)
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                    return BadRequest(new { message = "Incorrect current password" });
            }

            var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, newHashedPassword)
                .Inc(u => u.Logins, 1);

            await _context.Users.UpdateOneAsync(u => u.Id == id, update);

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
