using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using NOTIFICATIONSAPP.Services.Interfaces;
using NOTIFICATIONSAPP.Models;
/*
namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(
            INotificationService notificationService,
            IUserNotificationService userNotificationService,
            IConfiguration configuration,
            ILogger<WebhooksController> logger)
        {
            _notificationService = notificationService;
            _userNotificationService = userNotificationService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Slack webhook endpoint
        /// Docs: https://api.slack.com/apis/connections/events-api
        /// </summary>
        [HttpPost("slack")]
        public async Task<IActionResult> SlackWebhook([FromBody] JsonDocument payload)
        {
            try
            {
                // 1. Verify Slack signature
                if (!VerifySlackSignature(Request))
                {
                    _logger.LogWarning("Invalid Slack signature");
                    return Unauthorized("Invalid signature");
                }

                var root = payload.RootElement;

                // 2. Handle URL verification challenge (first-time setup)
                if (root.TryGetProperty("type", out var typeElement) && 
                    typeElement.GetString() == "url_verification")
                {
                    var challenge = root.GetProperty("challenge").GetString();
                    return Ok(new { challenge });
                }

                // 3. Extract event data
                if (!root.TryGetProperty("event", out var eventElement))
                {
                    return BadRequest("No event in payload");
                }

                var eventType = eventElement.GetProperty("type").GetString();
                var eventId = root.TryGetProperty("event_id", out var idEl) ? idEl.GetString() : null;

                // 4. Create notification (with deduplication via provider_event_id)
                var notification = new Notification
                {
                    Provider = "slack",
                    ProviderEventId = eventId,
                    Type = eventType,
                    Title = ExtractSlackTitle(eventElement),
                    Body = ExtractSlackBody(eventElement),
                    Payload = payload,
                    Status = "new",
                    ReceivedAt = DateTimeOffset.UtcNow
                };

                // Save (will throw if duplicate event_id due to unique index)
                try
                {
                    var created = await _notificationService.CreateAsync(notification);
                    
                    // Route to specific users based on channel/rules
                    await RouteNotificationToUsers(created, eventElement);
                    
                    _logger.LogInformation($"Slack notification created: {created.Id}");
                }
                catch (Exception ex) when (ex.Message.Contains("duplicate"))
                {
                    _logger.LogInformation($"Duplicate Slack event ignored: {eventId}");
                    return Ok(); // Already processed
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Slack webhook");
                return StatusCode(500, "Internal error");
            }
        }

        /// <summary>
        /// Discord webhook endpoint
        /// Docs: https://discord.com/developers/docs/topics/gateway-events
        /// </summary>
        [HttpPost("discord")]
        public async Task<IActionResult> DiscordWebhook([FromBody] JsonDocument payload)
        {
            try
            {
                // 1. Verify Discord signature
                if (!VerifyDiscordSignature(Request))
                {
                    _logger.LogWarning("Invalid Discord signature");
                    return Unauthorized("Invalid signature");
                }

                var root = payload.RootElement;

                // 2. Handle Discord interaction types
                if (root.TryGetProperty("type", out var typeElement))
                {
                    var type = typeElement.GetInt32();
                    
                    // Type 1 = PING (Discord bot verification)
                    if (type == 1)
                    {
                        return Ok(new { type = 1 }); // PONG
                    }
                }

                // 3. Extract event data
                var eventId = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                var eventType = root.TryGetProperty("t", out var tEl) ? tEl.GetString() : "unknown";

                // 4. Create notification
                var notification = new Notification
                {
                    Provider = "discord",
                    ProviderEventId = eventId,
                    Type = eventType,
                    Title = ExtractDiscordTitle(root),
                    Body = ExtractDiscordBody(root),
                    Payload = payload,
                    Status = "new",
                    ReceivedAt = DateTimeOffset.UtcNow
                };

                try
                {
                    var created = await _notificationService.CreateAsync(notification);
                    
                    // Route to specific users
                    await RouteNotificationToUsers(created, root);
                    
                    _logger.LogInformation($"Discord notification created: {created.Id}");
                }
                catch (Exception ex) when (ex.Message.Contains("duplicate"))
                {
                    _logger.LogInformation($"Duplicate Discord event ignored: {eventId}");
                    return Ok();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Discord webhook");
                return StatusCode(500, "Internal error");
            }
        }

        /// <summary>
        /// Facebook webhook endpoint (GET for verification, POST for events)
        /// Docs: https://developers.facebook.com/docs/graph-api/webhooks
        /// </summary>
        [HttpGet("facebook")]
        public IActionResult FacebookWebhookVerification(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            var expectedToken = _configuration["Facebook:VerifyToken"];
            
            if (mode == "subscribe" && verifyToken == expectedToken)
            {
                _logger.LogInformation("Facebook webhook verified");
                return Content(challenge, "text/plain");
            }

            return Unauthorized();
        }

        [HttpPost("facebook")]
        public async Task<IActionResult> FacebookWebhook([FromBody] JsonDocument payload)
        {
            try
            {
                // 1. Verify Facebook signature
                if (!VerifyFacebookSignature(Request))
                {
                    _logger.LogWarning("Invalid Facebook signature");
                    return Unauthorized("Invalid signature");
                }

                var root = payload.RootElement;

                // 2. Facebook sends batched entries
                if (!root.TryGetProperty("entry", out var entries))
                {
                    return BadRequest("No entries in payload");
                }

                foreach (var entry in entries.EnumerateArray())
                {
                    var entryId = entry.GetProperty("id").GetString();
                    
                    if (!entry.TryGetProperty("changes", out var changes))
                        continue;

                    foreach (var change in changes.EnumerateArray())
                    {
                        var field = change.GetProperty("field").GetString();
                        var value = change.GetProperty("value");

                        // Create unique event ID from entry + timestamp
                        var eventId = $"{entryId}_{entry.GetProperty("time").GetInt64()}";

                        var notification = new Notification
                        {
                            Provider = "facebook",
                            ProviderEventId = eventId,
                            Type = field,
                            Title = ExtractFacebookTitle(value, field),
                            Body = ExtractFacebookBody(value, field),
                            Payload = JsonDocument.Parse(value.GetRawText()),
                            Status = "new",
                            ReceivedAt = DateTimeOffset.UtcNow
                        };

                        try
                        {
                            var created = await _notificationService.CreateAsync(notification);
                            
                            // Route to specific users
                            await RouteNotificationToUsers(created, value);
                            
                            _logger.LogInformation($"Facebook notification created: {created.Id}");
                        }
                        catch (Exception ex) when (ex.Message.Contains("duplicate"))
                        {
                            _logger.LogInformation($"Duplicate Facebook event ignored: {eventId}");
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Facebook webhook");
                return StatusCode(500, "Internal error");
            }
        }

        /// <summary>
        /// Routes a notification to specific users based on configuration and rules
        /// </summary>
        private async Task RouteNotificationToUsers(Notification notification, JsonElement eventData)
        {
            try
            {
                var userIds = new List<Guid>();

                // Strategy 1: Check for mentioned users (e.g., @mentions in Slack/Discord)
                var mentionedUsers = ExtractMentionedUsers(eventData, notification.Provider);
                userIds.AddRange(mentionedUsers);

                // Strategy 2: Route by channel configuration
                if (notification.IntegrationChannelId.HasValue)
                {
                    var channelSubscribers = await GetChannelSubscribers(notification.IntegrationChannelId.Value);
                    userIds.AddRange(channelSubscribers);
                }

                // Strategy 3: Route by event type rules (from configuration)
                var ruleBasedUsers = GetUsersByEventTypeRules(notification.Provider, notification.Type);
                userIds.AddRange(ruleBasedUsers);

                // Strategy 4: Broadcast to all users if no specific routing (fallback)
                if (!userIds.Any())
                {
                    var broadcastEnabled = _configuration.GetValue<bool>($"{notification.Provider}:BroadcastToAllUsers");
                    if (broadcastEnabled)
                    {
                        userIds.AddRange(await GetAllUserIds());
                    }
                }

                // Remove duplicates and create UserNotification records
                var distinctUserIds = userIds.Distinct();
                foreach (var userId in distinctUserIds)
                {
                    var userNotification = new UserNotification
                    {
                        UserId = userId,
                        NotificationId = notification.Id,
                        IsRead = false,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _userNotificationService.CreateAsync(userNotification);
                }

                _logger.LogInformation($"Routed notification {notification.Id} to {distinctUserIds.Count()} users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error routing notification {notification.Id} to users");
            }
        }

        /// <summary>
        /// Extracts mentioned user IDs from the event payload
        /// </summary>
        private List<Guid> ExtractMentionedUsers(JsonElement eventData, string provider)
        {
            var userIds = new List<Guid>();

            try
            {
                switch (provider.ToLower())
                {
                    case "slack":
                        // Slack mentions format: <@U12345678>
                        if (eventData.TryGetProperty("text", out var slackText))
                        {
                            var text = slackText.GetString() ?? string.Empty;
                            var mentionPattern = @"<@(\w+)>";
                            var matches = System.Text.RegularExpressions.Regex.Matches(text, mentionPattern);
                            
                            foreach (System.Text.RegularExpressions.Match match in matches)
                            {
                                var slackUserId = match.Groups[1].Value;
                                var userId = MapExternalUserIdToInternalUserId(slackUserId, provider);
                                if (userId.HasValue)
                                    userIds.Add(userId.Value);
                            }
                        }
                        break;

                    case "discord":
                        // Discord mentions in content or mentions array
                        if (eventData.TryGetProperty("d", out var discordData))
                        {
                            if (discordData.TryGetProperty("mentions", out var mentions))
                            {
                                foreach (var mention in mentions.EnumerateArray())
                                {
                                    if (mention.TryGetProperty("id", out var discordUserId))
                                    {
                                        var userId = MapExternalUserIdToInternalUserId(discordUserId.GetString(), provider);
                                        if (userId.HasValue)
                                            userIds.Add(userId.Value);
                                    }
                                }
                            }
                        }
                        break;

                    case "facebook":
                        // Facebook tags in message
                        if (eventData.TryGetProperty("tags", out var tags))
                        {
                            foreach (var tag in tags.EnumerateArray())
                            {
                                if (tag.TryGetProperty("id", out var fbUserId))
                                {
                                    var userId = MapExternalUserIdToInternalUserId(fbUserId.GetString(), provider);
                                    if (userId.HasValue)
                                        userIds.Add(userId.Value);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error extracting mentioned users from {provider} event");
            }

            return userIds;
        }

        /// <summary>
        /// Maps external user ID (Slack/Discord/Facebook) to internal user ID
        /// You would typically store this mapping in a database table
        /// </summary>
        private Guid? MapExternalUserIdToInternalUserId(string? externalUserId, string provider)
        {
            if (string.IsNullOrEmpty(externalUserId))
                return null;

            // TODO: Implement actual mapping logic
            // This would query a user_external_accounts table that maps:
            // - user_id (internal)
            // - provider (slack/discord/facebook)
            // - external_user_id (provider's user ID)
            
            // For now, return null (no mapping found)
            return null;
        }

        /// <summary>
        /// Gets users subscribed to a specific channel
        /// </summary>
        private async Task<List<Guid>> GetChannelSubscribers(Guid channelId)
        {
            // TODO: Implement channel subscription logic
            // This would query a channel_subscriptions table
            // For now, return empty list
            return await Task.FromResult(new List<Guid>());
        }

        /// <summary>
        /// Gets users based on event type routing rules from configuration
        /// </summary>
        private List<Guid> GetUsersByEventTypeRules(string provider, string? eventType)
        {
            var userIds = new List<Guid>();

            if (string.IsNullOrEmpty(eventType))
                return userIds;

            try
            {
                // Check configuration for routing rules
                var rulesSection = _configuration.GetSection($"NotificationRouting:{provider}:{eventType}");
                var userIdsConfig = rulesSection.Get<string[]>();

                if (userIdsConfig != null)
                {
                    foreach (var userIdStr in userIdsConfig)
                    {
                        if (Guid.TryParse(userIdStr, out var userId))
                        {
                            userIds.Add(userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error getting users by event type rules for {provider}:{eventType}");
            }

            return userIds;
        }

        /// <summary>
        /// Gets all user IDs for broadcast scenarios
        /// </summary>
        private async Task<List<Guid>> GetAllUserIds()
        {
            // TODO: Implement by querying IUserService or adding a GetAllUserIds method
            // For now, return empty list to avoid broadcasting to everyone
            return await Task.FromResult(new List<Guid>());
        }


        private bool VerifySlackSignature(HttpRequest request)
        {
            var signingSecret = _configuration["Slack:SigningSecret"];
            if (string.IsNullOrEmpty(signingSecret))
                return true; // Skip in development

            var timestamp = request.Headers["X-Slack-Request-Timestamp"].ToString();
            var signature = request.Headers["X-Slack-Signature"].ToString();

            // Prevent replay attacks (timestamp should be within 5 minutes)
            if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(timestamp)) > 300)
                return false;

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = reader.ReadToEnd();
            request.Body.Position = 0;

            var baseString = $"v0:{timestamp}:{body}";
            var hash = ComputeHmacSha256(signingSecret, baseString);
            var expectedSignature = $"v0={hash}";

            return signature == expectedSignature;
        }

        private bool VerifyDiscordSignature(HttpRequest request)
        {
            var publicKey = _configuration["Discord:PublicKey"];
            if (string.IsNullOrEmpty(publicKey))
                return true; // Skip in development

            // Discord uses Ed25519 signature verification
            // You'll need a library like NSec or Sodium.Core for this
            // For now, return true (implement proper verification in production)
            
            _logger.LogWarning("Discord signature verification not implemented");
            return true;
        }

        private bool VerifyFacebookSignature(HttpRequest request)
        {
            var appSecret = _configuration["Facebook:AppSecret"];
            if (string.IsNullOrEmpty(appSecret))
                return true; // Skip in development

            var signature = request.Headers["X-Hub-Signature-256"].ToString();
            if (string.IsNullOrEmpty(signature))
                return false;

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = reader.ReadToEnd();
            request.Body.Position = 0;

            var hash = ComputeHmacSha256(appSecret, body);
            var expectedSignature = $"sha256={hash}";

            return signature == expectedSignature;
        }

        private string ComputeHmacSha256(string secret, string message)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }


        private string? ExtractSlackTitle(JsonElement eventElement)
        {
            var type = eventElement.GetProperty("type").GetString();
            return type switch
            {
                "message" => "New Slack Message",
                "reaction_added" => "Reaction Added",
                "app_mention" => "Mentioned in Slack",
                _ => $"Slack {type}"
            };
        }

        private string? ExtractSlackBody(JsonElement eventElement)
        {
            if (eventElement.TryGetProperty("text", out var text))
                return text.GetString();
            
            if (eventElement.TryGetProperty("message", out var message) &&
                message.TryGetProperty("text", out var msgText))
                return msgText.GetString();

            return null;
        }

        private string? ExtractDiscordTitle(JsonElement root)
        {
            if (root.TryGetProperty("d", out var data) &&
                data.TryGetProperty("content", out var content))
            {
                var text = content.GetString();
                return text?.Length > 50 ? text.Substring(0, 50) + "..." : text;
            }
            return "Discord Event";
        }

        private string? ExtractDiscordBody(JsonElement root)
        {
            if (root.TryGetProperty("d", out var data) &&
                data.TryGetProperty("content", out var content))
            {
                return content.GetString();
            }
            return null;
        }

        private string? ExtractFacebookTitle(JsonElement value, string? field)
        {
            return field switch
            {
                "comments" => "New Comment",
                "feed" => "New Post",
                "messages" => "New Message",
                _ => $"Facebook {field}"
            };
        }

        private string? ExtractFacebookBody(JsonElement value, string? field)
        {
            if (value.TryGetProperty("message", out var message))
                return message.GetString();
            
            if (value.TryGetProperty("text", out var text))
                return text.GetString();

            return null;
        }

    }
}*/