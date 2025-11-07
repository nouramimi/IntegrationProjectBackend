using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using NOTIFICATIONSAPP.Services.Interfaces;
using NOTIFICATIONSAPP.Models;
using Microsoft.AspNetCore.DataProtection;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;
        private readonly IIntegrationCredentialService _credentialService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OAuthController> _logger;
        private readonly IDataProtector _dataProtector;

        public OAuthController(
            IIntegrationService integrationService,
            IIntegrationCredentialService credentialService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<OAuthController> logger,
            IDataProtectionProvider dataProtectionProvider)
        {
            _integrationService = integrationService;
            _credentialService = credentialService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _dataProtector = dataProtectionProvider.CreateProtector("NOTIFICATIONSAPP.OAuth.Tokens");
        }

        #region Slack OAuth

        [HttpGet("slack/authorize")]
        public IActionResult SlackAuthorize([FromQuery] Guid? orgId, [FromQuery] string? state)
        {
            var clientId = _configuration["Slack:ClientId"];
            var redirectUri = _configuration["Slack:RedirectUri"];
            var scopes = "channels:read,chat:write,users:read"; // Adjust as needed

            var stateParam = state ?? Guid.NewGuid().ToString();
            
            // Store state + orgId in cache/session for validation in callback
            // HttpContext.Session.SetString($"slack_state_{stateParam}", orgId?.ToString() ?? "");

            var authUrl = $"https://slack.com/oauth/v2/authorize?" +
                         $"client_id={clientId}&" +
                         $"scope={Uri.EscapeDataString(scopes)}&" +
                         $"redirect_uri={Uri.EscapeDataString(redirectUri!)}&" +
                         $"state={stateParam}";

            return Redirect(authUrl);
        }

        [HttpGet("slack/callback")]
        public async Task<IActionResult> SlackCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError($"Slack OAuth error: {error}");
                return BadRequest($"OAuth failed: {error}");
            }

            try
            {
                var tokenResponse = await ExchangeSlackCode(code);
                
                if (tokenResponse == null)
                    return BadRequest("Failed to exchange code for token");

                var root = tokenResponse.RootElement;
                var accessToken = root.GetProperty("access_token").GetString();
                var teamId = root.GetProperty("team").GetProperty("id").GetString();
                var teamName = root.GetProperty("team").GetProperty("name").GetString();

                var integration = new Integration
                {
                    Provider = "slack",
                    ExternalAccountId = teamId,
                    Name = teamName,
                    IsActive = true,
                    Settings = JsonDocument.Parse("{}")
                };

                var createdIntegration = await _integrationService.CreateAsync(integration);

                 var credential = new IntegrationCredential
                {
                    IntegrationId = createdIntegration.Id,
                    CredentialType = "access_token",
                    Value = EncryptToken(accessToken!),
                    ExpiresAt = null, // Slack tokens don't expire by default
                    Meta = tokenResponse
                };

                await _credentialService.CreateAsync(credential);

                _logger.LogInformation($"Slack integration created: {createdIntegration.Id}");

                return Redirect($"/integrations/success?provider=slack&id={createdIntegration.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Slack OAuth callback");
                return StatusCode(500, "Failed to complete Slack authorization");
            }
        }

        private async Task<JsonDocument?> ExchangeSlackCode(string code)
        {
            var clientId = _configuration["Slack:ClientId"];
            var clientSecret = _configuration["Slack:ClientSecret"];
            var redirectUri = _configuration["Slack:RedirectUri"];

            var client = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri!)
            });

            var response = await client.PostAsync("https://slack.com/api/oauth.v2.access", content);
            var json = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(json);
        }

        #endregion

        #region Discord OAuth

        [HttpGet("discord/authorize")]
        public IActionResult DiscordAuthorize([FromQuery] Guid? orgId, [FromQuery] string? state)
        {
            var clientId = _configuration["Discord:ClientId"];
            var redirectUri = _configuration["Discord:RedirectUri"];
            var scopes = "identify guilds bot"; // Adjust as needed

            var stateParam = state ?? Guid.NewGuid().ToString();

            var authUrl = $"https://discord.com/api/oauth2/authorize?" +
                         $"client_id={clientId}&" +
                         $"redirect_uri={Uri.EscapeDataString(redirectUri!)}&" +
                         $"response_type=code&" +
                         $"scope={Uri.EscapeDataString(scopes)}&" +
                         $"state={stateParam}";

            return Redirect(authUrl);
        }

        [HttpGet("discord/callback")]
        public async Task<IActionResult> DiscordCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError($"Discord OAuth error: {error}");
                return BadRequest($"OAuth failed: {error}");
            }

            try
            {
                // 1. Exchange code for access token
                var tokenResponse = await ExchangeDiscordCode(code);
                
                if (tokenResponse == null)
                    return BadRequest("Failed to exchange code for token");

                var root = tokenResponse.RootElement;
                var accessToken = root.GetProperty("access_token").GetString();
                var refreshToken = root.GetProperty("refresh_token").GetString();
                var expiresIn = root.GetProperty("expires_in").GetInt32();

                // 2. Get user/guild info
                var userInfo = await GetDiscordUserInfo(accessToken!);
                var userId = userInfo?.RootElement.GetProperty("id").GetString();
                var username = userInfo?.RootElement.GetProperty("username").GetString();

                // 3. Create Integration record
                var integration = new Integration
                {
                    Provider = "discord",
                    ExternalAccountId = userId,
                    Name = $"Discord - {username}",
                    IsActive = true,
                    Settings = JsonDocument.Parse("{}")
                };

                var createdIntegration = await _integrationService.CreateAsync(integration);

                // 4. Store credentials
                await _credentialService.CreateAsync(new IntegrationCredential
                {
                    IntegrationId = createdIntegration.Id,
                    CredentialType = "access_token",
                    Value = EncryptToken(accessToken!),
                    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn),
                    Meta = tokenResponse
                });

                await _credentialService.CreateAsync(new IntegrationCredential
                {
                    IntegrationId = createdIntegration.Id,
                    CredentialType = "refresh_token",
                    Value = EncryptToken(refreshToken!),
                    ExpiresAt = null
                });

                _logger.LogInformation($"Discord integration created: {createdIntegration.Id}");

                return Redirect($"/integrations/success?provider=discord&id={createdIntegration.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Discord OAuth callback");
                return StatusCode(500, "Failed to complete Discord authorization");
            }
        }

        private async Task<JsonDocument?> ExchangeDiscordCode(string code)
        {
            var clientId = _configuration["Discord:ClientId"];
            var clientSecret = _configuration["Discord:ClientSecret"];
            var redirectUri = _configuration["Discord:RedirectUri"];

            var client = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri!)
            });

            var response = await client.PostAsync("https://discord.com/api/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(json);
        }

        private async Task<JsonDocument?> GetDiscordUserInfo(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await client.GetAsync("https://discord.com/api/users/@me");
            var json = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(json);
        }

        #endregion

        #region Facebook OAuth

        [HttpGet("facebook/authorize")]
        public IActionResult FacebookAuthorize([FromQuery] Guid? orgId, [FromQuery] string? state)
        {
            var appId = _configuration["Facebook:AppId"];
            var redirectUri = _configuration["Facebook:RedirectUri"];
            var scopes = "pages_manage_posts,pages_read_engagement"; // Adjust as needed

            var stateParam = state ?? Guid.NewGuid().ToString();

            var authUrl = $"https://www.facebook.com/v18.0/dialog/oauth?" +
                         $"client_id={appId}&" +
                         $"redirect_uri={Uri.EscapeDataString(redirectUri!)}&" +
                         $"scope={Uri.EscapeDataString(scopes)}&" +
                         $"state={stateParam}";

            return Redirect(authUrl);
        }

        [HttpGet("facebook/callback")]
        public async Task<IActionResult> FacebookCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError($"Facebook OAuth error: {error}");
                return BadRequest($"OAuth failed: {error}");
            }

            try
            {
                // 1. Exchange code for access token
                var tokenResponse = await ExchangeFacebookCode(code);
                
                if (tokenResponse == null)
                    return BadRequest("Failed to exchange code for token");

                var root = tokenResponse.RootElement;
                var accessToken = root.GetProperty("access_token").GetString();
                var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 0;

                // 2. Get user/page info
                var userInfo = await GetFacebookUserInfo(accessToken!);
                var userId = userInfo?.RootElement.GetProperty("id").GetString();
                var userName = userInfo?.RootElement.GetProperty("name").GetString();

                // 3. Create Integration record
                var integration = new Integration
                {
                    Provider = "facebook",
                    ExternalAccountId = userId,
                    Name = $"Facebook - {userName}",
                    IsActive = true,
                    Settings = JsonDocument.Parse("{}")
                };

                var createdIntegration = await _integrationService.CreateAsync(integration);

                // 4. Store credentials
                await _credentialService.CreateAsync(new IntegrationCredential
                {
                    IntegrationId = createdIntegration.Id,
                    CredentialType = "access_token",
                    Value = EncryptToken(accessToken!),
                    ExpiresAt = expiresIn > 0 ? DateTimeOffset.UtcNow.AddSeconds(expiresIn) : null,
                    Meta = tokenResponse
                });

                _logger.LogInformation($"Facebook integration created: {createdIntegration.Id}");

                return Redirect($"/integrations/success?provider=facebook&id={createdIntegration.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Facebook OAuth callback");
                return StatusCode(500, "Failed to complete Facebook authorization");
            }
        }

        private async Task<JsonDocument?> ExchangeFacebookCode(string code)
        {
            var appId = _configuration["Facebook:AppId"];
            var appSecret = _configuration["Facebook:AppSecret"];
            var redirectUri = _configuration["Facebook:RedirectUri"];

            var client = _httpClientFactory.CreateClient();
            var url = $"https://graph.facebook.com/v18.0/oauth/access_token?" +
                     $"client_id={appId}&" +
                     $"client_secret={appSecret}&" +
                     $"redirect_uri={Uri.EscapeDataString(redirectUri!)}&" +
                     $"code={code}";

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(json);
        }

        private async Task<JsonDocument?> GetFacebookUserInfo(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://graph.facebook.com/v18.0/me?access_token={accessToken}";

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(json);
        }

        #endregion

        #region Helper Methods

        private string EncryptToken(string token)
        {
            return _dataProtector.Protect(token);
        }

        #endregion
    }
}