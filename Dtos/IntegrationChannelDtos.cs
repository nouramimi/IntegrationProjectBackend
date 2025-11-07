using System.Text.Json;

namespace NOTIFICATIONSAPP.Dtos
{
    public class IntegrationChannelReadDto
    {
        public Guid Id { get; set; }
        public Guid IntegrationId { get; set; }
        public string ExternalChannelId { get; set; } = null!;
        public string? Name { get; set; }
        public JsonDocument? Config { get; set; }   // ✅ FIXED
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class IntegrationChannelCreateDto
    {
        public Guid IntegrationId { get; set; }       // ✅ FIXED
        public string ExternalChannelId { get; set; } = null!;
        public string? Name { get; set; }
        public JsonDocument? Config { get; set; }     // ✅ FIXED
    }

    public class IntegrationChannelUpdateDto
    {
        public string? Name { get; set; }
        public JsonDocument? Config { get; set; }     // ✅ FIXED
        public bool? IsActive { get; set; }
    }
}
