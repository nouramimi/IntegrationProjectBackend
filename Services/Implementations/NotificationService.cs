using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Notification?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<Notification> CreateAsync(Notification notification)
        {
            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> UpdateAsync(Notification notification)
        {
            _repository.Update(notification);
            await _repository.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            _repository.Remove(entity);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
