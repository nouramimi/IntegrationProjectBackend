using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONSAPP.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace NOTIFICATIONSAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationFixController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IIntegrationService _integrationService;
        private readonly ILogger<NotificationFixController> _logger;

        public NotificationFixController(
            INotificationService notificationService,
            IIntegrationService integrationService,
            ILogger<NotificationFixController> logger)
        {
            _notificationService = notificationService;
            _integrationService = integrationService;
            _logger = logger;
        }

        /// <summary>
        /// Fix notifications with missing IntegrationId by matching team_id from payload
        /// </summary>
        [HttpPost("fix-missing-integration-ids")]
        public async Task<IActionResult> FixMissingIntegrationIds()
        {
            var allNotifications = await _notificationService.GetAllAsync();
            var allIntegrations = await _integrationService.GetAllAsync();
            
            var notificationsWithoutIntegration = allNotifications
                .Where(n => n.IntegrationId == null && n.Provider == "slack")
                .ToList();

            var fixedCount = 0;

            foreach (var notification in notificationsWithoutIntegration)
            {
                try
                {
                    // Extract team_id from payload
                    if (notification.Payload?.RootElement.TryGetProperty("team_id", out var teamIdElement) == true)
                    {
                        var teamId = teamIdElement.GetString();
                        
                        // Find matching integration
                        var integration = allIntegrations.FirstOrDefault(i => 
                            i.Provider == "slack" && 
                            i.ExternalAccountId == teamId &&
                            i.IsActive);

                        if (integration != null)
                        {
                            notification.IntegrationId = integration.Id;
                            await _notificationService.UpdateAsync(notification);
                            fixedCount++;
                            
                            _logger.LogInformation($"Fixed notification {notification.Id} - assigned integration {integration.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"No integration found for team_id: {teamId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error fixing notification {notification.Id}");
                }
            }

            return Ok(new 
            { 
                message = $"Fixed {fixedCount} notifications",
                totalChecked = notificationsWithoutIntegration.Count
            });
        }
    }
}