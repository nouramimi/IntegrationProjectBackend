using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface IIntegrationCredentialService
    {
        Task<IEnumerable<IntegrationCredential>> GetAllAsync();
        Task<IntegrationCredential?> GetByIdAsync(Guid id);
        Task<IntegrationCredential> CreateAsync(IntegrationCredential credential);
        Task<IntegrationCredential> UpdateAsync(IntegrationCredential credential);
        Task<bool> DeleteAsync(Guid id);
    }
}
