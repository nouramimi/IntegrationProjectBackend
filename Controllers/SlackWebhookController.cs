using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NOTIFICATIONSAPP.Services.Interfaces;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Data;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace NOTIFICATIONSAPP.Webhooks
{
    [ApiController]
    [Route("api/webhooks/slack")]
    public class SlackWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IIntegrationService _integrationService;
        private readonly IAttachmentService _attachmentService;
        private readonly ILogger<SlackWebhookController> _logger;
        private readonly INotificationRouterService _notificationRouter;
        private readonly AppDbContext _context;

        public SlackWebhookController(
            IConfiguration configuration,
            INotificationService notificationService,
            IIntegrationService integrationService,
            IAttachmentService attachmentService,
            INotificationRouterService notificationRouter,
            AppDbContext context,
            ILogger<SlackWebhookController> logger)
        {
            _configuration = configuration;
            _notificationService = notificationService;
            _integrationService = integrationService;
            _attachmentService = attachmentService;
            _notificationRouter = notificationRouter; 
            _context = context; 
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> Post()
        {
            // Read the request body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            _logger.LogInformation($"Received Slack webhook: {body}");

            // Verify Slack signature
            if (!VerifySlackSignature(Request, body))
            {
                _logger.LogWarning("Invalid Slack signature");
                return Unauthorized("Invalid signature");
            }

            // Parse JSON payload
            using var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            // 1️⃣ URL verification (Slack challenge)
            if (root.TryGetProperty("type", out var type) && type.GetString() == "url_verification")
            {
                var challenge = root.GetProperty("challenge").GetString();
                _logger.LogInformation("Slack URL verification successful");
                return Ok(new { challenge });
            }

            // 2️⃣ Handle event callbacks
            if (type.GetString() == "event_callback")
            {
                var teamId = root.GetProperty("team_id").GetString();
                _logger.LogInformation($"📌 Slack Team ID: {teamId}");
                await HandleEventCallback(root);
            }

            return Ok();
        }

        private async Task HandleEventCallback(JsonElement root)
        {
            try
            {
                var eventData = root.GetProperty("event");
                var eventType = eventData.GetProperty("type").GetString();
                var teamId = root.GetProperty("team_id").GetString();

                _logger.LogInformation($"Processing Slack event: {eventType} from team: {teamId}");

                // Find or create the integration by team_id (ExternalAccountId)
                var integrations = await _integrationService.GetAllAsync();
                var integration = integrations.FirstOrDefault(i =>
                    i.Provider == "slack" &&
                    i.ExternalAccountId == teamId &&
                    i.IsActive);

                if (integration == null)
                {
                    _logger.LogInformation($"No integration found for team {teamId}, creating new one...");

                    // Auto-create integration
                    integration = new Integration
                    {
                        Provider = "slack",
                        ExternalAccountId = teamId,
                        Name = $"Slack Workspace {teamId}",
                        IsActive = true,
                        Settings = JsonDocument.Parse("{}")
                    };

                    integration = await _integrationService.CreateAsync(integration);
                    _logger.LogInformation($"Created Slack integration: {integration.Id} for team: {teamId}");
                }

                // Handle different event types
                switch (eventType)
                {
                    case "message":
                        await HandleMessageEvent(eventData, integration);
                        break;

                    case "app_mention":
                        await HandleAppMentionEvent(eventData, integration);
                        break;

                    case "reaction_added":
                        await HandleReactionEvent(eventData, integration);
                        break;

                    default:
                        _logger.LogInformation($"Unhandled Slack event type: {eventType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Slack event callback");
            }
        }


        private async Task HandleMessageEvent(JsonElement eventData, Integration integration)
        {
            try
            {
                var userId = eventData.GetProperty("user").GetString();
                var channelId = eventData.GetProperty("channel").GetString();
                var text = eventData.GetProperty("text").GetString();
                var timestamp = eventData.GetProperty("ts").GetString();

                if (eventData.TryGetProperty("bot_id", out _))
                {
                    _logger.LogInformation("Skipping bot message");
                    return;
                }

                // 🔍 Trouver ou créer le canal
                var channel = await FindOrCreateChannel(integration.Id, channelId);

                var payloadObject = new Dictionary<string, object>
                {
                    ["channel_id"] = channelId,
                    ["user_id"] = userId,
                    ["timestamp"] = timestamp,
                    ["event_type"] = "message",
                    ["text"] = text
                };

                if (eventData.TryGetProperty("files", out var filesInPayload))
                {
                    payloadObject["files"] = JsonSerializer.Deserialize<JsonElement>(filesInPayload.GetRawText());
                }

                if (eventData.TryGetProperty("blocks", out var blocksElement))
                {
                    payloadObject["blocks"] = JsonSerializer.Deserialize<JsonElement>(blocksElement.GetRawText());
                }

                if (eventData.TryGetProperty("subtype", out var subtypeElement))
                {
                    payloadObject["subtype"] = subtypeElement.GetString();
                }

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    IntegrationChannelId = channel.Id, // ✨ NOUVEAU
                    Provider = "slack",
                    ProviderEventId = timestamp,
                    Type = "message",
                    Title = $"New Slack message in #{channel.Name ?? channelId}",
                    Body = text ?? "(file shared)",
                    Status = "new",
                    ExternalUserId = userId, // ✨ NOUVEAU - Pour le routage
                    ExternalChannelId = channelId, // ✨ NOUVEAU - Pour le routage
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(payloadObject))
                };

                var createdNotification = await _notificationService.CreateAsync(notification);

                // 🚀 ROUTAGE AUTOMATIQUE vers les users concernés
                await _notificationRouter.RouteNotificationAsync(createdNotification);

                if (eventData.TryGetProperty("files", out var filesElement) &&
                    filesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var file in filesElement.EnumerateArray())
                    {
                        await ProcessFileAttachment(file, createdNotification.Id);
                    }
                }

                _logger.LogInformation($"✅ Stored and routed Slack message notification: {createdNotification.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Slack message event");
            }
        }
        
        private async Task<IntegrationChannel> FindOrCreateChannel(Guid integrationId, string externalChannelId)
        {
            var channel = await _context.IntegrationChannels
                .FirstOrDefaultAsync(ic => 
                    ic.IntegrationId == integrationId && 
                    ic.ExternalChannelId == externalChannelId);

            if (channel == null)
            {
                _logger.LogInformation($"Creating new channel for {externalChannelId}");
                
                channel = new IntegrationChannel
                {
                    IntegrationId = integrationId,
                    ExternalChannelId = externalChannelId,
                    Name = $"#{externalChannelId}",
                    Type = "unknown", // Vous pouvez utiliser l'API Slack pour récupérer le nom réel
                    IsActive = true,
                    Config = JsonDocument.Parse("{}")
                };

                _context.IntegrationChannels.Add(channel);
                await _context.SaveChangesAsync();
            }

            return channel;
        }

        private async Task ProcessFileAttachment(JsonElement file, Guid notificationId)
        {
            try
            {
                var fileId = file.GetProperty("id").GetString();
                var fileName = file.GetProperty("name").GetString();
                var mimeType = file.GetProperty("mimetype").GetString();
                var fileSize = file.GetProperty("size").GetInt64();
                var urlPrivate = file.GetProperty("url_private").GetString();
                var permalink = file.GetProperty("permalink").GetString();

                var attachment = new Attachment
                {
                    NotificationId = notificationId,
                    Filename = fileName,
                    ContentType = mimeType,
                    Size = fileSize,
                    Url = urlPrivate,
                    Meta = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        file_id = fileId,
                        permalink = permalink,
                        url_private_download = file.TryGetProperty("url_private_download", out var downloadUrl) 
                            ? downloadUrl.GetString() 
                            : null
                    }))
                };

                await _attachmentService.CreateAsync(attachment);
                _logger.LogInformation($"📎 Saved attachment: {fileName} for notification {notificationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file attachment");
            }
        }

        private async Task HandleAppMentionEvent(JsonElement eventData, Integration integration)
        {
            try
            {
                var userId = eventData.GetProperty("user").GetString();
                var channelId = eventData.GetProperty("channel").GetString();
                var text = eventData.GetProperty("text").GetString();
                var timestamp = eventData.GetProperty("ts").GetString();

                var channel = await FindOrCreateChannel(integration.Id, channelId);

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    IntegrationChannelId = channel.Id,
                    Provider = "slack",
                    ProviderEventId = timestamp,
                    Type = "app_mention",
                    Title = "You were mentioned in Slack",
                    Body = text,
                    Status = "new",
                    ExternalUserId = userId,
                    ExternalChannelId = channelId,
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        channel_id = channelId,
                        user_id = userId,
                        timestamp = timestamp,
                        event_type = "app_mention",
                        text = text
                    }))
                };

                var createdNotification = await _notificationService.CreateAsync(notification);
                await _notificationRouter.RouteNotificationAsync(createdNotification);

                _logger.LogInformation($"✅ Stored and routed Slack mention notification: {createdNotification.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Slack app mention event");
            }
        }

        private async Task HandleReactionEvent(JsonElement eventData, Integration integration)
        {
            try
            {
                var userId = eventData.GetProperty("user").GetString();
                var reaction = eventData.GetProperty("reaction").GetString();
                var itemUser = eventData.GetProperty("item_user").GetString();
                
                var item = eventData.GetProperty("item");
                var channelId = item.GetProperty("channel").GetString();
                var timestamp = item.GetProperty("ts").GetString();

                var channel = await FindOrCreateChannel(integration.Id, channelId);

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    IntegrationChannelId = channel.Id,
                    Provider = "slack",
                    ProviderEventId = $"{timestamp}_{reaction}",
                    Type = "reaction_added",
                    Title = "Someone reacted to your message",
                    Body = $"Reacted with :{reaction}:",
                    Status = "new",
                    ExternalUserId = itemUser, // ✨ L'utilisateur qui a écrit le message original
                    ExternalChannelId = channelId,
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        channel_id = channelId,
                        user_id = userId, // Celui qui a réagi
                        item_user = itemUser, // L'auteur du message
                        reaction = reaction,
                        timestamp = timestamp,
                        event_type = "reaction_added"
                    }))
                };

                var createdNotification = await _notificationService.CreateAsync(notification);
                await _notificationRouter.RouteNotificationAsync(createdNotification);

                _logger.LogInformation($"✅ Stored and routed Slack reaction notification: {createdNotification.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Slack reaction event");
            }
        }

        private bool VerifySlackSignature(HttpRequest request, string body)
        {
            var signingSecret = _configuration["Slack:SigningSecret"];
            if (string.IsNullOrEmpty(signingSecret))
            {
                _logger.LogWarning("Slack signing secret not configured - skipping verification");
                return true; // Skip verification if missing (for local tests)
            }

            var timestamp = request.Headers["X-Slack-Request-Timestamp"].ToString();
            var signature = request.Headers["X-Slack-Signature"].ToString();

            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Missing Slack signature headers");
                return false;
            }

            // Prevent replay attacks (older than 5 min)
            if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(timestamp)) > 300)
            {
                _logger.LogWarning("Slack request timestamp too old");
                return false;
            }

            var baseString = $"v0:{timestamp}:{body}";
            var hash = ComputeHmacSha256(signingSecret, baseString);
            var expectedSignature = $"v0={hash}";

            var isValid = signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
                _logger.LogWarning("Slack signature verification failed");

            return isValid;
        }

        private string ComputeHmacSha256(string secret, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}