using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class DeliveryAttemptService : IDeliveryAttemptService
    {
        private readonly IDeliveryAttemptRepository _repository;

        public DeliveryAttemptService(IDeliveryAttemptRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DeliveryAttempt>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<DeliveryAttempt?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<DeliveryAttempt> CreateAsync(DeliveryAttempt attempt)
        {
            await _repository.AddAsync(attempt);
            await _repository.SaveChangesAsync();
            return attempt;
        }

        public async Task<DeliveryAttempt> UpdateAsync(DeliveryAttempt attempt)
        {
            _repository.Update(attempt);
            await _repository.SaveChangesAsync();
            return attempt;
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
