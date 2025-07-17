using MyMongoApp.Enums;

namespace MyMongoApp.Dtos
{
    public class CreateLoginRuleDto
    {
        public List<string> UserIds { get; set; } = new();
        public LoginRulesRestriction Restriction { get; set; } = LoginRulesRestriction.Unknown;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
