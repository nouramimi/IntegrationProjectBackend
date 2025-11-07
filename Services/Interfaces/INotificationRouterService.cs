using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface INotificationRouterService
    {
        Task RouteNotificationAsync(Notification notification);
        Task RouteNotificationToSpecificUsersAsync(Guid notificationId, List<Guid> userIds);
        Task<List<Guid>> GetEligibleUsersForNotificationAsync(Guid notificationId);
    }
}
