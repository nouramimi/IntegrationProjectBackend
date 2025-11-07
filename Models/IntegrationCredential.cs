using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class IntegrationCredential
    {
        public Guid Id { get; set; }
        public Guid IntegrationId { get; set; }
        public Integration Integration { get; set; } = null!;
        public string CredentialType { get; set; } = null!;
        public string Value { get; set; } = null!;
        public DateTimeOffset? ExpiresAt { get; set; }
        public JsonDocument? Meta { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

}