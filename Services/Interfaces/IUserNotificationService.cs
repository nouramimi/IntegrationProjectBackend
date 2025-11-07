using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface IUserNotificationService
    {
        Task<IEnumerable<UserNotification>> GetAllAsync();
        Task<UserNotification?> GetByIdAsync(Guid id);
        Task<UserNotification> CreateAsync(UserNotification userNotification);
        Task<UserNotification> UpdateAsync(UserNotification userNotification);
        Task<bool> DeleteAsync(Guid id);
        
    }
}
