using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Data
{
    public class AppDbContext : DbContext
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Payload)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Integration>()
                .Property(i => i.Settings)
                .HasColumnType("jsonb");

            modelBuilder.Entity<IntegrationChannel>()
                .Property(c => c.Config)
                .HasColumnType("jsonb");

            modelBuilder.Entity<IntegrationCredential>()
                .Property(c => c.Meta)
                .HasColumnType("jsonb");
            
            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.User)
                .WithMany(u => u.UserNotifications)
                .HasForeignKey(un => un.UserId)
                .OnDelete(DeleteBehavior.Cascade); 

            
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Notification)
                .WithMany(n => n.Attachments)
                .HasForeignKey(a => a.NotificationId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<IntegrationChannel>()
                .HasOne(c => c.Integration)
                .WithMany(i => i.Channels)
                .HasForeignKey(c => c.IntegrationId)
                .OnDelete(DeleteBehavior.Cascade); 
            
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Integration)
                .WithMany(i => i.Notifications)
                .HasForeignKey(n => n.IntegrationId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.IntegrationChannel)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.IntegrationChannelId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<IntegrationCredential>()
                .HasOne(c => c.Integration)
                .WithMany(i => i.Credentials)
                .HasForeignKey(c => c.IntegrationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeliveryAttempt>()
                .HasOne(d => d.Notification)
                .WithMany(n => n.DeliveryAttempts)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.Notification)
                .WithMany(n => n.UserNotifications)
                .HasForeignKey(un => un.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.ReceivedAt);


            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.Provider, n.ProviderEventId })
                .IsUnique()
                .HasFilter("\"ProviderEventId\" IS NOT NULL");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Username).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<OrganizationUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Organization)
                    .WithMany(o => o.OrganizationUsers)
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("member");

                entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
            });
            modelBuilder.Entity<Integration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ExternalAccountId).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(e => e.Organization)
                    .WithMany(o => o.Integrations)
                    .HasForeignKey(e => e.OrgId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.Provider, e.ExternalAccountId });
            });
            modelBuilder.Entity<UserIntegration>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserIntegrations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Integration)
                    .WithMany(i => i.UserIntegrations)
                    .HasForeignKey(e => e.IntegrationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ExternalUserId).HasMaxLength(255);
                entity.Property(e => e.ExternalUsername).HasMaxLength(255);
                entity.Property(e => e.Settings).HasColumnType("jsonb");

                entity.HasIndex(e => new { e.UserId, e.IntegrationId }).IsUnique();
                entity.HasIndex(e => e.ExternalUserId);
            });

            // Configuration IntegrationCredential
            modelBuilder.Entity<IntegrationCredential>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CredentialType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Meta).HasColumnType("jsonb");

                entity.HasOne(e => e.Integration)
                    .WithMany(i => i.Credentials)
                    .HasForeignKey(e => e.IntegrationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration IntegrationChannel
            modelBuilder.Entity<IntegrationChannel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExternalChannelId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Config).HasColumnType("jsonb");

                entity.HasOne(e => e.Integration)
                    .WithMany(i => i.Channels)
                    .HasForeignKey(e => e.IntegrationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.IntegrationId, e.ExternalChannelId }).IsUnique();
            });

            modelBuilder.Entity<UserChannel>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserChannels)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.IntegrationChannel)
                    .WithMany(ic => ic.UserChannels)
                    .HasForeignKey(e => e.IntegrationChannelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.IntegrationChannelId }).IsUnique();
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProviderEventId).HasMaxLength(255);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("new");
                entity.Property(e => e.Payload).HasColumnType("jsonb").IsRequired();
                
                entity.Property(e => e.ExternalUserId).HasMaxLength(255);
                entity.Property(e => e.ExternalChannelId).HasMaxLength(255);

                entity.HasOne(e => e.Integration)
                    .WithMany(i => i.Notifications)
                    .HasForeignKey(e => e.IntegrationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.IntegrationChannel)
                    .WithMany(ic => ic.Notifications)
                    .HasForeignKey(e => e.IntegrationChannelId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.ProviderEventId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ReceivedAt);
                entity.HasIndex(e => e.ExternalUserId);
                entity.HasIndex(e => e.ExternalChannelId);
            });

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Filename).HasMaxLength(500);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.Meta).HasColumnType("jsonb");

                entity.HasOne(e => e.Notification)
                    .WithMany(n => n.Attachments)
                    .HasForeignKey(e => e.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserNotifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Notification)
                    .WithMany(n => n.UserNotifications)
                    .HasForeignKey(e => e.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.NotificationId }).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.IsRead });
                entity.HasIndex(e => e.CreatedAt);
            });

            modelBuilder.Entity<DeliveryAttempt>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(e => e.Notification)
                    .WithMany(n => n.DeliveryAttempts)
                    .HasForeignKey(e => e.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.AttemptedAt);
            });

        }


        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<Integration> Integrations { get; set; } = null!;
        public DbSet<IntegrationChannel> IntegrationChannels { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;
        public DbSet<UserNotification> UserNotifications { get; set; } = null!;
        public DbSet<DeliveryAttempt> DeliveryAttempts { get; set; } = null!;
        public DbSet<IntegrationCredential> IntegrationCredentials { get; set; } = null!;

        public DbSet<OrganizationUser> OrganizationUsers { get; set; }
        public DbSet<UserIntegration> UserIntegrations { get; set; }
        public DbSet<UserChannel> UserChannels { get; set; }

    }
}