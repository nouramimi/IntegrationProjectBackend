using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class IntegrationChannel
    {
        public Guid Id { get; set; }
        public Guid IntegrationId { get; set; }
        public Integration Integration { get; set; } = null!;
        public string ExternalChannelId { get; set; } = null!; // channel_id Slack/Discord
        public string? Name { get; set; }
        public string? Type { get; set; } // "public", "private", "dm"
        public JsonDocument? Config { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<UserChannel> UserChannels { get; set; } = new List<UserChannel>(); // <-- add this
    }
}
