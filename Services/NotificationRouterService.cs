using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Data;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Services.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NOTIFICATIONSAPP.Services
{
    public class NotificationRouterService : INotificationRouterService
    {
        private readonly AppDbContext _context;
        private readonly IUserNotificationService _userNotificationService;
        private readonly ILogger<NotificationRouterService> _logger;

        public NotificationRouterService(
            AppDbContext context,
            IUserNotificationService userNotificationService,
            ILogger<NotificationRouterService> logger)
        {
            _context = context;
            _userNotificationService = userNotificationService;
            _logger = logger;
        }

        public async Task RouteNotificationAsync(Notification notification)
        {
            try
            {
                var userIds = new HashSet<Guid>();

                // 1️⃣ Router vers l'auteur du message
                await RouteToAuthor(notification, userIds);

                // 2️⃣ Router vers tous les users abonnés au canal
                await RouteToChannelSubscribers(notification, userIds);

                // 3️⃣ Router vers les users mentionnés (@mentions)
                await RouteToMentionedUsers(notification, userIds);

                // 4️⃣ Créer les UserNotifications pour tous les users concernés
                await CreateUserNotifications(notification.Id, userIds);

                _logger.LogInformation($"✅ Routed notification {notification.Id} to {userIds.Count} users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error routing notification {notification.Id}");
            }
        }

        private async Task RouteToAuthor(Notification notification, HashSet<Guid> userIds)
        {
            if (string.IsNullOrEmpty(notification.ExternalUserId)) return;

            var userIntegration = await _context.UserIntegrations
                .FirstOrDefaultAsync(ui => 
                    ui.IntegrationId == notification.IntegrationId &&
                    ui.ExternalUserId == notification.ExternalUserId &&
                    ui.ReceiveNotifications);

            if (userIntegration != null)
            {
                userIds.Add(userIntegration.UserId);
                _logger.LogInformation($"📝 Routing to author: User {userIntegration.UserId}");
            }
        }

        private async Task RouteToChannelSubscribers(Notification notification, HashSet<Guid> userIds)
        {
            if (notification.IntegrationChannelId == null) return;

            var channelSubscribers = await _context.UserChannels
                .Where(uc => 
                    uc.IntegrationChannelId == notification.IntegrationChannelId &&
                    uc.IsMember &&
                    uc.ReceiveNotifications)
                .Select(uc => uc.UserId)
                .ToListAsync();

            foreach (var userId in channelSubscribers)
            {
                userIds.Add(userId);
            }

            _logger.LogInformation($"📢 Routing to {channelSubscribers.Count} channel subscribers");
        }

        private async Task RouteToMentionedUsers(Notification notification, HashSet<Guid> userIds)
        {
            try
            {
                var mentionedUserIds = ExtractMentions(notification);
                if (!mentionedUserIds.Any()) return;

                var mentionedUsers = await _context.UserIntegrations
                    .Where(ui => 
                        ui.IntegrationId == notification.IntegrationId &&
                        mentionedUserIds.Contains(ui.ExternalUserId) &&
                        ui.ReceiveNotifications)
                    .Select(ui => ui.UserId)
                    .ToListAsync();

                foreach (var userId in mentionedUsers)
                {
                    userIds.Add(userId);
                }

                _logger.LogInformation($"💬 Routing to {mentionedUsers.Count} mentioned users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting mentions");
            }
        }

        private List<string> ExtractMentions(Notification notification)
        {
            var mentions = new List<string>();

            try
            {
                var payload = notification.Payload.RootElement;

                // Slack mentions: <@U12345678>
                if (notification.Provider == "slack")
                {
                    if (payload.TryGetProperty("text", out var text))
                    {
                        var slackMentions = Regex.Matches(text.GetString() ?? "", @"<@(U[A-Z0-9]+)>")
                            .Select(m => m.Groups[1].Value);
                        mentions.AddRange(slackMentions);
                    }
                }

                // Discord mentions: dans le tableau mentions
                if (notification.Provider == "discord")
                {
                    if (payload.TryGetProperty("mentions", out var mentionsArray))
                    {
                        foreach (var mention in mentionsArray.EnumerateArray())
                        {
                            if (mention.TryGetProperty("id", out var id))
                            {
                                mentions.Add(id.GetString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing mentions from payload");
            }

            return mentions;
        }

        private async Task CreateUserNotifications(Guid notificationId, HashSet<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                var userNotification = new UserNotification
                {
                    UserId = userId,
                    NotificationId = notificationId,
                    IsRead = false
                };

                await _userNotificationService.CreateAsync(userNotification);
            }
        }

        public async Task RouteNotificationToSpecificUsersAsync(
            Guid notificationId, 
            List<Guid> userIds)
        {
            await CreateUserNotifications(notificationId, userIds.ToHashSet());
        }

        public async Task<List<Guid>> GetEligibleUsersForNotificationAsync(Guid notificationId)
        {
            var notification = await _context.Notifications
                .Include(n => n.Integration)
                .Include(n => n.IntegrationChannel)
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null) return new List<Guid>();

            var userIds = new HashSet<Guid>();
            await RouteToAuthor(notification, userIds);
            await RouteToChannelSubscribers(notification, userIds);
            await RouteToMentionedUsers(notification, userIds);

            return userIds.ToList();
        }
    }
}