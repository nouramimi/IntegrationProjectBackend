using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class UserChannel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid IntegrationChannelId { get; set; }
        public IntegrationChannel IntegrationChannel { get; set; } = null!;

        public bool IsMember { get; set; } = true;
        public bool ReceiveNotifications { get; set; } = true;
        public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
