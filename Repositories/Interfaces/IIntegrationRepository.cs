using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IIntegrationRepository
    {
        Task<Integration?> GetByIdAsync(Guid id);
        Task<IEnumerable<Integration>> GetAllAsync();
        Task AddAsync(Integration integration);
        void Update(Integration integration);
        void Remove(Integration integration);
        Task SaveChangesAsync();
    }
}
