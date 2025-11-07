using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        public ICollection<UserIntegration> UserIntegrations { get; set; } = new List<UserIntegration>();
        public ICollection<UserChannel> UserChannels { get; set; } = new List<UserChannel>();
    }

}