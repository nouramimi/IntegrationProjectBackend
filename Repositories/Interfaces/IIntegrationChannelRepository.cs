using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IIntegrationChannelRepository
    {
        Task<IntegrationChannel?> GetByIdAsync(Guid id);
        Task<IEnumerable<IntegrationChannel>> GetByIntegrationIdAsync(Guid integrationId);
        Task AddAsync(IntegrationChannel channel);
        void Update(IntegrationChannel channel);
        void Remove(IntegrationChannel channel);
        Task SaveChangesAsync();
        Task<IEnumerable<IntegrationChannel>> GetAllAsync();

    }
}
