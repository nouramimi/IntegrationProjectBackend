using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class OrganizationUser
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Role { get; set; } = "member"; // admin, member, viewer
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
    