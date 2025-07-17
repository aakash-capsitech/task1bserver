using MyMongoApp.Enums;

namespace MyMongoApp.Dtos
{
    public class CreateLoginRuleDto
    {
        public string UserId { get; set; } = string.Empty;
        public LoginRulesRestriction Restriction { get; set; } = LoginRulesRestriction.Unknown;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
