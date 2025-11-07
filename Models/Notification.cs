using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid? IntegrationId { get; set; }
        public Integration? Integration { get; set; }
        public Guid? IntegrationChannelId { get; set; }
        public IntegrationChannel? IntegrationChannel { get; set; }
        public string Provider { get; set; } = null!;
        public string? ProviderEventId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public JsonDocument Payload { get; set; } = null!;
        public string Status { get; set; } = "new";

        public string? ExternalUserId { get; set; }
        public string? ExternalChannelId { get; set; }
        public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ProcessedAt { get; set; }

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        public ICollection<DeliveryAttempt> DeliveryAttempts { get; set; } = new List<DeliveryAttempt>();
    }

}