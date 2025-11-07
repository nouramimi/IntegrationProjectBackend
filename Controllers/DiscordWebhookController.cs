using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NOTIFICATIONSAPP.Services.Interfaces;
using NOTIFICATIONSAPP.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace NOTIFICATIONSAPP.Webhooks
{
    [ApiController]
    [Route("api/webhooks/discord")]
    public class DiscordWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IIntegrationService _integrationService;
        private readonly IAttachmentService _attachmentService;
        private readonly ILogger<DiscordWebhookController> _logger;

        public DiscordWebhookController(
            IConfiguration configuration,
            INotificationService notificationService,
            IIntegrationService integrationService,
            IAttachmentService attachmentService,
            ILogger<DiscordWebhookController> logger)
        {
            _configuration = configuration;
            _notificationService = notificationService;
            _integrationService = integrationService;
            _attachmentService = attachmentService;
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

            _logger.LogInformation($"Received Discord webhook: {body}");

            // Verify Discord signature (Ed25519)
            if (!VerifyDiscordSignature(Request, body))
            {
                _logger.LogWarning("Invalid Discord signature");
                return Unauthorized(new { error = "Invalid signature" });
            }

            // Parse JSON payload
            using var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            // Get interaction type
            if (!root.TryGetProperty("type", out var typeElement))
            {
                _logger.LogWarning("Discord payload missing 'type' field");
                return BadRequest("Missing type field");
            }

            var interactionType = typeElement.GetInt32();

            // 1️⃣ PING (Type 1) - Discord verification
            if (interactionType == 1)
            {
                _logger.LogInformation("✅ Discord PING verification successful");
                return Ok(new { type = 1 }); // PONG response
            }

            // 2️⃣ APPLICATION_COMMAND (Type 2) - Slash commands
            if (interactionType == 2)
            {
                await HandleApplicationCommand(root);
                return Ok(new
                {
                    type = 4, // CHANNEL_MESSAGE_WITH_SOURCE
                    data = new { content = "Command received!" }
                });
            }

            // 3️⃣ MESSAGE_COMPONENT (Type 3) - Button clicks, select menus
            if (interactionType == 3)
            {
                await HandleMessageComponent(root);
                return Ok(new { type = 6 }); // DEFERRED_UPDATE_MESSAGE
            }

            // For other webhook events (if using gateway events)
            await HandleGatewayEvent(root);

            return Ok();
        }

        private async Task HandleApplicationCommand(JsonElement root)
        {
            try
            {
                var data = root.GetProperty("data");
                var commandName = data.GetProperty("name").GetString();
                var guildId = root.TryGetProperty("guild_id", out var guild) ? guild.GetString() : null;
                var channelId = root.GetProperty("channel_id").GetString();
                var userId = root.GetProperty("member").GetProperty("user").GetProperty("id").GetString();
                var username = root.GetProperty("member").GetProperty("user").GetProperty("username").GetString();

                _logger.LogInformation($"Processing Discord command: {commandName} from guild: {guildId}");

                // Find or create integration
                var integration = await FindOrCreateIntegration(guildId);

                // Create notification for slash command
                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    Provider = "discord",
                    ProviderEventId = root.GetProperty("id").GetString(),
                    Type = "slash_command",
                    Title = $"Discord Command: /{commandName}",
                    Body = $"User {username} used /{commandName}",
                    Status = "new",
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        command_name = commandName,
                        guild_id = guildId,
                        channel_id = channelId,
                        user_id = userId,
                        username = username,
                        interaction_id = root.GetProperty("id").GetString(),
                        event_type = "slash_command"
                    }))
                };

                var createdNotification = await _notificationService.CreateAsync(notification);
                _logger.LogInformation($"✅ Stored Discord command notification: {createdNotification.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Discord application command");
            }
        }

        private async Task HandleMessageComponent(JsonElement root)
        {
            try
            {
                var data = root.GetProperty("data");
                var customId = data.GetProperty("custom_id").GetString();
                var componentType = data.GetProperty("component_type").GetInt32(); // 2=Button, 3=Select
                var guildId = root.TryGetProperty("guild_id", out var guild) ? guild.GetString() : null;
                var channelId = root.GetProperty("channel_id").GetString();
                var userId = root.GetProperty("member").GetProperty("user").GetProperty("id").GetString();

                _logger.LogInformation($"Processing Discord component: {customId} (type: {componentType})");

                var integration = await FindOrCreateIntegration(guildId);

                var componentTypeName = componentType switch
                {
                    2 => "button",
                    3 => "select_menu",
                    _ => "unknown"
                };

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    Provider = "discord",
                    ProviderEventId = root.GetProperty("id").GetString(),
                    Type = "component_interaction",
                    Title = $"Discord {componentTypeName} clicked",
                    Body = $"Component: {customId}",
                    Status = "new",
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        custom_id = customId,
                        component_type = componentTypeName,
                        guild_id = guildId,
                        channel_id = channelId,
                        user_id = userId,
                        interaction_id = root.GetProperty("id").GetString(),
                        event_type = "component_interaction"
                    }))
                };

                await _notificationService.CreateAsync(notification);
                _logger.LogInformation($"✅ Stored Discord component notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Discord message component");
            }
        }

        private async Task HandleGatewayEvent(JsonElement root)
        {
            try
            {
                // Handle Discord Gateway events (MESSAGE_CREATE, MESSAGE_UPDATE, etc.)
                if (!root.TryGetProperty("t", out var eventTypeElement))
                    return;

                var eventType = eventTypeElement.GetString();
                var eventData = root.GetProperty("d");

                _logger.LogInformation($"Processing Discord gateway event: {eventType}");

                switch (eventType)
                {
                    case "MESSAGE_CREATE":
                        await HandleMessageCreate(eventData);
                        break;

                    case "MESSAGE_UPDATE":
                        await HandleMessageUpdate(eventData);
                        break;

                    case "MESSAGE_REACTION_ADD":
                        await HandleReactionAdd(eventData);
                        break;

                    case "GUILD_MEMBER_ADD":
                        await HandleMemberJoin(eventData);
                        break;

                    default:
                        _logger.LogInformation($"Unhandled Discord event type: {eventType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Discord gateway event");
            }
        }

        private async Task HandleMessageCreate(JsonElement eventData)
        {
            try
            {
                var messageId = eventData.GetProperty("id").GetString();
                var channelId = eventData.GetProperty("channel_id").GetString();
                var content = eventData.TryGetProperty("content", out var cont) ? cont.GetString() : null;
                var author = eventData.GetProperty("author");
                var authorId = author.GetProperty("id").GetString();
                var authorUsername = author.GetProperty("username").GetString();
                var isBot = author.TryGetProperty("bot", out var bot) && bot.GetBoolean();

                // Skip bot messages
                if (isBot)
                {
                    _logger.LogInformation("Skipping Discord bot message");
                    return;
                }

                var guildId = eventData.TryGetProperty("guild_id", out var guild) ? guild.GetString() : null;
                var integration = await FindOrCreateIntegration(guildId);

                // Create payload with all message data
                var payloadObject = new Dictionary<string, object>
                {
                    ["message_id"] = messageId,
                    ["channel_id"] = channelId,
                    ["guild_id"] = guildId,
                    ["author_id"] = authorId,
                    ["author_username"] = authorUsername,
                    ["content"] = content ?? "(no text content)",
                    ["timestamp"] = eventData.GetProperty("timestamp").GetString(),
                    ["event_type"] = "message_create"
                };

                // Add embeds if present
                if (eventData.TryGetProperty("embeds", out var embedsElement) && embedsElement.GetArrayLength() > 0)
                {
                    payloadObject["embeds"] = JsonSerializer.Deserialize<JsonElement>(embedsElement.GetRawText());
                }

                // Add mentions if present
                if (eventData.TryGetProperty("mentions", out var mentionsElement) && mentionsElement.GetArrayLength() > 0)
                {
                    payloadObject["mentions"] = JsonSerializer.Deserialize<JsonElement>(mentionsElement.GetRawText());
                }

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    Provider = "discord",
                    ProviderEventId = messageId,
                    Type = "message",
                    Title = $"New Discord message in #{channelId}",
                    Body = content ?? "(file/embed shared)",
                    Status = "new",
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(payloadObject))
                };

                var createdNotification = await _notificationService.CreateAsync(notification);

                // Handle attachments if present
                if (eventData.TryGetProperty("attachments", out var attachmentsElement) &&
                    attachmentsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var attachment in attachmentsElement.EnumerateArray())
                    {
                        await ProcessAttachment(attachment, createdNotification.Id);
                    }
                }

                _logger.LogInformation($"✅ Stored Discord message notification: {createdNotification.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Discord message create");
            }
        }

        private async Task ProcessAttachment(JsonElement attachment, Guid notificationId)
        {
            try
            {
                var attachmentId = attachment.GetProperty("id").GetString();
                var filename = attachment.GetProperty("filename").GetString();
                var contentType = attachment.TryGetProperty("content_type", out var ct) ? ct.GetString() : null;
                var size = attachment.GetProperty("size").GetInt64();
                var url = attachment.GetProperty("url").GetString();
                var proxyUrl = attachment.GetProperty("proxy_url").GetString();

                var attachmentRecord = new Attachment
                {
                    NotificationId = notificationId,
                    Filename = filename,
                    ContentType = contentType,
                    Size = size,
                    Url = url,
                    Meta = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        attachment_id = attachmentId,
                        proxy_url = proxyUrl,
                        width = attachment.TryGetProperty("width", out var w) ? w.GetInt32() : (int?)null,
                        height = attachment.TryGetProperty("height", out var h) ? h.GetInt32() : (int?)null
                    }))
                };

                await _attachmentService.CreateAsync(attachmentRecord);
                _logger.LogInformation($"📎 Saved Discord attachment: {filename} for notification {notificationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Discord attachment");
            }
        }

        private async Task HandleMessageUpdate(JsonElement eventData)
        {
            try
            {
                var messageId = eventData.GetProperty("id").GetString();
                var channelId = eventData.GetProperty("channel_id").GetString();
                var content = eventData.TryGetProperty("content", out var cont) ? cont.GetString() : null;
                var guildId = eventData.TryGetProperty("guild_id", out var guild) ? guild.GetString() : null;

                var integration = await FindOrCreateIntegration(guildId);

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    Provider = "discord",
                    ProviderEventId = $"{messageId}_edit",
                    Type = "message_update",
                    Title = "Discord message edited",
                    Body = content ?? "(content unavailable)",
                    Status = "new",
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        message_id = messageId,
                        channel_id = channelId,
                        guild_id = guildId,
                        content = content,
                        event_type = "message_update"
                    }))
                };

                await _notificationService.CreateAsync(notification);
                _logger.LogInformation($"✅ Stored Discord message edit notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Discord message update");
            }
        }

        private async Task HandleReactionAdd(JsonElement eventData)
        {
            try
            {
                var userId = eventData.GetProperty("user_id").GetString();
                var channelId = eventData.GetProperty("channel_id").GetString();
                var messageId = eventData.GetProperty("message_id").GetString();
                var guildId = eventData.TryGetProperty("guild_id", out var guild) ? guild.GetString() : null;

                var emoji = eventData.GetProperty("emoji");
                var emojiName = emoji.GetProperty("name").GetString();
                var emojiId = emoji.TryGetProperty("id", out var id) ? id.GetString() : null;

                var integration = await FindOrCreateIntegration(guildId);

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    Provider = "discord",
                    ProviderEventId = $"{messageId}_{emojiName}_{userId}",
                    Type = "reaction_add",
                    Title = "Someone reacted to a message",
                    Body = $"Reacted with {emojiName}",
                    Status = "new",
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        user_id = userId,
                        channel_id = channelId,
                        message_id = messageId,
                        guild_id = guildId,
                        emoji_name = emojiName,
                        emoji_id = emojiId,
                        event_type = "reaction_add"
                    }))
                };

                await _notificationService.CreateAsync(notification);
                _logger.LogInformation($"✅ Stored Discord reaction notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Discord reaction add");
            }
        }

        private async Task HandleMemberJoin(JsonElement eventData)
        {
            try
            {
                var user = eventData.GetProperty("user");
                var userId = user.GetProperty("id").GetString();
                var username = user.GetProperty("username").GetString();
                var guildId = eventData.GetProperty("guild_id").GetString();

                var integration = await FindOrCreateIntegration(guildId);

                var notification = new Notification
                {
                    IntegrationId = integration.Id,
                    Provider = "discord",
                    ProviderEventId = $"{guildId}_{userId}_join",
                    Type = "member_join",
                    Title = "New member joined",
                    Body = $"{username} joined the server",
                    Status = "new",
                    Payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        user_id = userId,
                        username = username,
                        guild_id = guildId,
                        joined_at = eventData.GetProperty("joined_at").GetString(),
                        event_type = "member_join"
                    }))
                };

                await _notificationService.CreateAsync(notification);
                _logger.LogInformation($"✅ Stored Discord member join notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Discord member join");
            }
        }

        private async Task<Integration> FindOrCreateIntegration(string? guildId)
        {
            var integrations = await _integrationService.GetAllAsync();
            var integration = integrations.FirstOrDefault(i =>
                i.Provider == "discord" &&
                i.ExternalAccountId == guildId &&
                i.IsActive);

            if (integration == null)
            {
                _logger.LogInformation($"No integration found for Discord guild {guildId}, creating new one...");

                integration = new Integration
                {
                    Provider = "discord",
                    ExternalAccountId = guildId ?? "dm",
                    Name = $"Discord Server {guildId ?? "DM"}",
                    IsActive = true,
                    Settings = JsonDocument.Parse("{}")
                };

                integration = await _integrationService.CreateAsync(integration);
                _logger.LogInformation($"Created Discord integration: {integration.Id} for guild: {guildId}");
            }

            return integration;
        }

        private bool VerifyDiscordSignature(HttpRequest request, string body)
{
    var publicKey = _configuration["Discord:PublicKey"];
    if (string.IsNullOrEmpty(publicKey))
    {
        _logger.LogError("Discord public key missing in configuration");
        return false;
    }

    var signature = request.Headers["X-Signature-Ed25519"].ToString();
    var timestamp = request.Headers["X-Signature-Timestamp"].ToString();

    if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp))
    {
        _logger.LogWarning("Missing Discord signature headers");
        return false;
    }

    try
    {
        var message = Encoding.UTF8.GetBytes(timestamp + body);
        var signatureBytes = Convert.FromHexString(signature);
        var publicKeyBytes = Convert.FromHexString(publicKey);

        bool verified = Sodium.PublicKeyAuth.VerifyDetached(signatureBytes, message, publicKeyBytes);
        _logger.LogInformation($"Discord signature verified: {verified}");
        return verified;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error verifying Discord signature");
        return false;
    }
}



    }
}