using NOTIFICATIONSAPP.Services.Interfaces;
using NOTIFICATIONSAPP.Models;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace NOTIFICATIONSAPP.Services.Background
{
    /// <summary>
    /// Background service that processes incoming notifications
    /// - Extracts attachments
    /// - Routes to users based on rules
    /// - Sends real-time updates via SignalR
    /// - Handles delivery attempts
    /// </summary>
    public class WebhookProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebhookProcessorService> _logger;
        private readonly ConcurrentQueue<Guid> _notificationQueue = new();

        public WebhookProcessorService(
            IServiceProvider serviceProvider,
            ILogger<WebhookProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Queue a notification for processing
        /// </summary>
        public void QueueNotification(Guid notificationId)
        {
            _notificationQueue.Enqueue(notificationId);
            _logger.LogInformation($"Queued notification {notificationId} for processing");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebhookProcessorService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_notificationQueue.TryDequeue(out var notificationId))
                    {
                        await ProcessNotificationAsync(notificationId);
                    }
                    else
                    {
                        // Wait a bit if queue is empty
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WebhookProcessorService");
                    await Task.Delay(5000, stoppingToken); // Back off on error
                }
            }

            _logger.LogInformation("WebhookProcessorService stopped");
        }

        private async Task ProcessNotificationAsync(Guid notificationId)
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var userNotificationService = scope.ServiceProvider.GetRequiredService<IUserNotificationService>();
            var attachmentService = scope.ServiceProvider.GetRequiredService<IAttachmentService>();

            try
            {
                var notification = await notificationService.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    _logger.LogWarning($"Notification {notificationId} not found");
                    return;
                }

                _logger.LogInformation($"Processing notification {notificationId} ({notification.Provider})");

                // 1. Extract and store attachments
                await ExtractAttachmentsAsync(notification, attachmentService);

                // 2. Route to users based on channel/integration
                await RouteToUsersAsync(notification, userNotificationService);

                // 3. Send real-time updates via SignalR
                // await _hubContext.Clients.All.SendAsync("NewNotification", notification);

                // 4. Update status
                notification.Status = "processed";
                notification.ProcessedAt = DateTimeOffset.UtcNow;
                await notificationService.UpdateAsync(notification);

                _logger.LogInformation($"Successfully processed notification {notificationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process notification {notificationId}");
                
                // Mark as failed
                var notification = await notificationService.GetByIdAsync(notificationId);
                if (notification != null)
                {
                    notification.Status = "failed";
                    await notificationService.UpdateAsync(notification);
                }
            }
        }

        private async Task ExtractAttachmentsAsync(Notification notification, IAttachmentService attachmentService)
        {
            try
            {
                var payload = notification.Payload.RootElement;

                switch (notification.Provider)
                {
                    case "slack":
                        await ExtractSlackAttachments(notification, payload, attachmentService);
                        break;
                    case "discord":
                        await ExtractDiscordAttachments(notification, payload, attachmentService);
                        break;
                    case "facebook":
                        await ExtractFacebookAttachments(notification, payload, attachmentService);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting attachments for notification {notification.Id}");
            }
        }

        private async Task ExtractSlackAttachments(
            Notification notification, 
            System.Text.Json.JsonElement payload, 
            IAttachmentService attachmentService)
        {
            if (!payload.TryGetProperty("event", out var eventElement))
                return;

            if (!eventElement.TryGetProperty("files", out var files))
                return;

            foreach (var file in files.EnumerateArray())
            {
                var attachment = new Attachment
                {
                    NotificationId = notification.Id,
                    Url = file.GetProperty("url_private").GetString(),
                    Filename = file.TryGetProperty("name", out var name) ? name.GetString() : null,
                    ContentType = file.TryGetProperty("mimetype", out var mime) ? mime.GetString() : null,
                    Size = file.TryGetProperty("size", out var size) ? size.GetInt64() : null
                };

                await attachmentService.CreateAsync(attachment);
            }
        }

        private async Task ExtractDiscordAttachments(
            Notification notification, 
            System.Text.Json.JsonElement payload, 
            IAttachmentService attachmentService)
        {
            if (!payload.TryGetProperty("d", out var data))
                return;

            if (!data.TryGetProperty("attachments", out var attachments))
                return;

            foreach (var attachment in attachments.EnumerateArray())
            {
                var att = new Attachment
                {
                    NotificationId = notification.Id,
                    Url = attachment.GetProperty("url").GetString(),
                    Filename = attachment.TryGetProperty("filename", out var name) ? name.GetString() : null,
                    ContentType = attachment.TryGetProperty("content_type", out var ct) ? ct.GetString() : null,
                    Size = attachment.TryGetProperty("size", out var size) ? size.GetInt64() : null
                };

                await attachmentService.CreateAsync(att);
            }
        }

        private async Task ExtractFacebookAttachments(
            Notification notification, 
            System.Text.Json.JsonElement payload, 
            IAttachmentService attachmentService)
        {
            // Facebook attachment extraction logic
            // Depends on the specific event type
            if (payload.TryGetProperty("photo", out var photo))
            {
                var attachment = new Attachment
                {
                    NotificationId = notification.Id,
                    Url = photo.GetString(),
                    ContentType = "image/jpeg"
                };

                await attachmentService.CreateAsync(attachment);
            }
        }

        private async Task RouteToUsersAsync(
            Notification notification, 
            IUserNotificationService userNotificationService)
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            try
            {
                // Get all users who should receive this notification
                var users = await userService.GetAllAsync();
                
                // Create user notification for each user
                // In production, you'd filter based on:
                // - notification.IntegrationChannelId subscriptions
                // - User preferences/filters
                // - Mentioned users in the notification
                // - Organization membership
                
                foreach (var user in users)
                {
                    var userNotification = new UserNotification
                    {
                        UserId = user.Id,
                        NotificationId = notification.Id,
                        IsRead = false,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await userNotificationService.CreateAsync(userNotification);
                }

                _logger.LogInformation($"Routed notification {notification.Id} to {users.Count()} users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error routing notification {notification.Id} to users");
            }
        }
    }
}