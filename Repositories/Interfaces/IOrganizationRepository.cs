using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByIdAsync(Guid id);
        Task<IEnumerable<Organization>> GetAllAsync();
        Task AddAsync(Organization org);
        void Update(Organization org);
        void Remove(Organization org);
        Task SaveChangesAsync();
    }
}
