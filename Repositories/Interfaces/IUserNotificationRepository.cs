using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IUserNotificationRepository
    {
        Task<UserNotification?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserNotification>> GetByUserIdAsync(Guid userId);
        Task AddAsync(UserNotification un);
        void Update(UserNotification un);
        void Remove(UserNotification un);
        Task SaveChangesAsync();
        Task<IEnumerable<UserNotification>> GetAllAsync();
        
    }
}
