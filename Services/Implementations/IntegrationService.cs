using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class IntegrationService : IIntegrationService
    {
        private readonly IIntegrationRepository _repository;

        public IntegrationService(IIntegrationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Integration>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Integration?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<Integration> CreateAsync(Integration integration)
        {
            await _repository.AddAsync(integration);
            await _repository.SaveChangesAsync();
            return integration;
        }

        public async Task<Integration> UpdateAsync(Integration integration)
        {
            _repository.Update(integration);
            await _repository.SaveChangesAsync();
            return integration;
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
