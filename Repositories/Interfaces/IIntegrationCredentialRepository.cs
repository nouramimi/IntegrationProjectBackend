using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IIntegrationCredentialRepository
    {
        Task<IntegrationCredential?> GetByIdAsync(Guid id);
        Task<IEnumerable<IntegrationCredential>> GetByIntegrationIdAsync(Guid integrationId);
        Task AddAsync(IntegrationCredential credential);
        void Update(IntegrationCredential credential);
        void Remove(IntegrationCredential credential);
        Task SaveChangesAsync();
        Task<IEnumerable<IntegrationCredential>> GetAllAsync();

    }
}
