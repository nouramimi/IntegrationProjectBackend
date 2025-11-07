using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class UserNotification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTimeOffset? ReadAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

}