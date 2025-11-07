using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface IIntegrationChannelService
    {
        Task<IEnumerable<IntegrationChannel>> GetAllAsync();
        Task<IntegrationChannel?> GetByIdAsync(Guid id);
        Task<IntegrationChannel> CreateAsync(IntegrationChannel channel);
        Task<IntegrationChannel> UpdateAsync(IntegrationChannel channel);
        Task<bool> DeleteAsync(Guid id);
    }
}
