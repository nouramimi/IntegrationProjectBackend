using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class Integration
    {
        public Guid Id { get; set; }
        public Guid? OrgId { get; set; }
        public Organization? Organization { get; set; }
        public string Provider { get; set; } = null!;
        public string? ExternalAccountId { get; set; }
        public string? Name { get; set; }
        public JsonDocument? Settings { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<IntegrationCredential> Credentials { get; set; } = new List<IntegrationCredential>();
        public ICollection<IntegrationChannel> Channels { get; set; } = new List<IntegrationChannel>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<UserIntegration> UserIntegrations { get; set; } = new List<UserIntegration>();
    }

}