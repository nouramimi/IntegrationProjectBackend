using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface IDeliveryAttemptService
    {
        Task<IEnumerable<DeliveryAttempt>> GetAllAsync();
        Task<DeliveryAttempt?> GetByIdAsync(Guid id);
        Task<DeliveryAttempt> CreateAsync(DeliveryAttempt attempt);
        Task<DeliveryAttempt> UpdateAsync(DeliveryAttempt attempt);
        Task<bool> DeleteAsync(Guid id);
    }
}
