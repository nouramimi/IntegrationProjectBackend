using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class IntegrationChannelService : IIntegrationChannelService
    {
        private readonly IIntegrationChannelRepository _repository;

        public IntegrationChannelService(IIntegrationChannelRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<IntegrationChannel>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<IntegrationChannel?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<IntegrationChannel> CreateAsync(IntegrationChannel channel)
        {
            await _repository.AddAsync(channel);
            await _repository.SaveChangesAsync();
            return channel;
        }

        public async Task<IntegrationChannel> UpdateAsync(IntegrationChannel channel)
        {
            _repository.Update(channel);
            await _repository.SaveChangesAsync();
            return channel;
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
