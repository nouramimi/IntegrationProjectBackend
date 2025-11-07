using System.Text.Json;

namespace NOTIFICATIONSAPP.Dtos
{
    public class NotificationReadDto
    {
        public Guid Id { get; set; }
        public Guid? IntegrationId { get; set; }
        public Guid? IntegrationChannelId { get; set; }
        public string Provider { get; set; } = null!;
        public string? ProviderEventId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public JsonDocument Payload { get; set; } = null!; // ✅ FIXED
        public string Status { get; set; } = "new";
        public DateTimeOffset ReceivedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
    }

    public class NotificationCreateDto
    {
        public Guid? IntegrationId { get; set; }
        public Guid? IntegrationChannelId { get; set; }
        public string Provider { get; set; } = null!;
        public string? ProviderEventId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public JsonDocument Payload { get; set; } = null!; // ✅ FIXED
    }

    public class NotificationUpdateDto
    {
        public string? Status { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public JsonDocument? Payload { get; set; }       // ✅ FIXED
    }
}
