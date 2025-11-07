namespace NOTIFICATIONSAPP.Dtos
{
    public class OrganizationCreateDto
    {
        public string Name { get; set; } = null!;
    }

    public class OrganizationUpdateDto
    {
        public string? Name { get; set; }
    }

    public class OrganizationReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
