using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IUserNotificationRepository _repository;

        public UserNotificationService(IUserNotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserNotification>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<UserNotification?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<UserNotification> CreateAsync(UserNotification userNotification)
        {
            await _repository.AddAsync(userNotification);
            await _repository.SaveChangesAsync();
            return userNotification;
        }

        public async Task<UserNotification> UpdateAsync(UserNotification userNotification)
        {
            _repository.Update(userNotification);
            await _repository.SaveChangesAsync();
            return userNotification;
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
