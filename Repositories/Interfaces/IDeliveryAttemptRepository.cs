using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IDeliveryAttemptRepository
    {
        Task<DeliveryAttempt?> GetByIdAsync(Guid id);
        Task<IEnumerable<DeliveryAttempt>> GetByNotificationIdAsync(Guid notificationId);
        Task AddAsync(DeliveryAttempt attempt);
        void Update(DeliveryAttempt attempt);
        void Remove(DeliveryAttempt attempt);
        Task SaveChangesAsync();
        Task<IEnumerable<DeliveryAttempt>> GetAllAsync();
        

    }
}
