using System.Text.Json;

namespace NOTIFICATIONSAPP.Dtos
{
    public class IntegrationReadDto
    {
        public Guid Id { get; set; }
        public Guid? OrgId { get; set; }
        public string Provider { get; set; } = null!;
        public string? ExternalAccountId { get; set; }
        public string? Name { get; set; }
        public JsonDocument? Settings { get; set; }   // ✅ FIXED
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class IntegrationCreateDto
    {
        public Guid? OrgId { get; set; }
        public string Provider { get; set; } = null!;
        public string? ExternalAccountId { get; set; }
        public string? Name { get; set; }
        public JsonDocument? Settings { get; set; }   // ✅ FIXED
    }

    public class IntegrationUpdateDto
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public JsonDocument? Settings { get; set; }   // ✅ FIXED
    }
}
