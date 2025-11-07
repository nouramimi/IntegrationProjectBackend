using System.Text.Json;

namespace NOTIFICATIONSAPP.Dtos
{
    public class IntegrationCredentialReadDto
    {
        public Guid Id { get; set; }
        public Guid IntegrationId { get; set; }
        public string CredentialType { get; set; } = null!;
        public string Value { get; set; } = null!;
        public DateTimeOffset? ExpiresAt { get; set; }
        public JsonDocument? Meta { get; set; }       // ✅ FIXED
        public DateTimeOffset CreatedAt { get; set; } // ✅ FIXED
    }

    public class IntegrationCredentialCreateDto
    {
        public Guid IntegrationId { get; set; }       // ✅ FIXED
        public string CredentialType { get; set; } = null!;
        public string Value { get; set; } = null!;
        public DateTimeOffset? ExpiresAt { get; set; }
        public JsonDocument? Meta { get; set; }       // ✅ FIXED
    }
}
