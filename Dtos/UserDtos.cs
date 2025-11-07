namespace NOTIFICATIONSAPP.Dtos
{
    public class UserReadDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Username { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class UserCreateDto
    {
        public string Email { get; set; } = null!;
        public string? Username { get; set; }
        public string? Password { get; set; } // plain text before hashing
    }

    public class UserUpdateDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
