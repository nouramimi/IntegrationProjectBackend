using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class UserIntegration
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid IntegrationId { get; set; }
        public Integration Integration { get; set; } = null!;

        public string? ExternalUserId { get; set; } // slack_user_id ou discord_user_id
        public string? ExternalUsername { get; set; }
        public JsonDocument? Settings { get; set; } // Préférences de notification par intégration
        public bool ReceiveNotifications { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
