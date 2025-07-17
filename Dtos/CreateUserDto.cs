// namespace MyMongoApp.Dtos
// {
//     public class CreateUserDto
// {
//     public string Name { get; set; } = string.Empty;
//     public string Email { get; set; } = string.Empty;
//     public string Role { get; set; } = string.Empty;
//     public List<string> ConfigRoles { get; set; } = new();
// }

// }




















using MyMongoApp.Enums;

namespace MyMongoApp.Dtos
{
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Unknown;
        public string Phone { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<UserConfigRole> ConfigRoles { get; set; } = new();
    }
}
