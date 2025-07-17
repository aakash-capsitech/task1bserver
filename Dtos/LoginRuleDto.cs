public class LoginRuleDto
{
    public string Id { get; set; } = null!;
    public string Restriction { get; set; } = null!;
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public List<string> UserEmails { get; set; } = new();
}
