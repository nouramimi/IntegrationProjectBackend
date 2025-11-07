using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetAllAsync();
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> UpdateAsync(Notification notification);
        Task<bool> DeleteAsync(Guid id);
    }
}
