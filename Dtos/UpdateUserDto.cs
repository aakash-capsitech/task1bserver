using MyMongoApp.Enums;

namespace MyMongoApp.Dtos
{
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public string? Phone { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }
        public List<UserConfigRole>? ConfigRoles { get; set; }
    }
}
