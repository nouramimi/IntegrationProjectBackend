using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NOTIFICATIONSAPP.Models
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Integration> Integrations { get; set; } = new List<Integration>();
        public ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();
    }

}