// using MyMongoApp.Data; // ✅ This imports MongoDbContext
// using System.Security.Claims;


// public class RequestLoggingMiddleware
// {
//     private readonly RequestDelegate _next;

//     public RequestLoggingMiddleware(RequestDelegate next)
//     {
//         _next = next;
//     }

//     public async Task InvokeAsync(HttpContext context, MongoDbContext dbContext)
//     {
//         var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
//         var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
//         var role = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "unknown";
//         var path = context.Request.Path;
//         var method = context.Request.Method;

//         var log = new LogEntry
//         {
//             UserId = userId,
//             Email = email,
//             Role = role,
//             Path = path,
//             Method = method,
//             Timestamp = DateTime.UtcNow
//         };

//         try
//         {
//             await _next(context);
//             log.Status = context.Response.StatusCode.ToString();
//         }
//         catch (Exception ex)
//         {
//             log.Status = "Error";
//             log.ErrorMessage = ex.Message;
//             throw;
//         }
//         finally
//         {
//             await dbContext.Logs.InsertOneAsync(log);
//         }
//     }
// }
