using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface IIntegrationService
    {
        Task<IEnumerable<Integration>> GetAllAsync();
        Task<Integration?> GetByIdAsync(Guid id);
        Task<Integration> CreateAsync(Integration integration);
        Task<Integration> UpdateAsync(Integration integration);
        Task<bool> DeleteAsync(Guid id);
    }
}
