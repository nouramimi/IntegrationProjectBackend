using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notification>> GetAllAsync();
        Task AddAsync(Notification notification);
        void Update(Notification notification);
        void Remove(Notification notification);
        Task SaveChangesAsync();
    }
}
