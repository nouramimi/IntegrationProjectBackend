using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;
        public string? Url { get; set; }
        public string? Filename { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public JsonDocument? Meta { get; set; }
    }

}