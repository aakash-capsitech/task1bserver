using MongoDB.Bson;


public class LogEntry
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
}
