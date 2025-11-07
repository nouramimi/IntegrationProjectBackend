using System.Text.Json;

namespace NOTIFICATIONSAPP.Dtos
{
    public class AttachmentReadDto
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public string? Url { get; set; }
        public string? Filename { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public JsonDocument? Meta { get; set; } // ✅ FIXED
    }

    public class AttachmentCreateDto
    {
        public Guid NotificationId { get; set; }
        public string? Url { get; set; }
        public string? Filename { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public JsonDocument? Meta { get; set; } // ✅ FIXED
    }
}
