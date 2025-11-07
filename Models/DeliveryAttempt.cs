using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class DeliveryAttempt
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;
        public string? TargetUrl { get; set; }
        public string? Status { get; set; }
        public int? HttpStatus { get; set; }
        public string? ResponseBody { get; set; }
        public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
    }

}