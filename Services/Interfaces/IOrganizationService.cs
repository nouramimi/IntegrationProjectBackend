using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<Organization>> GetAllAsync();
        Task<Organization?> GetByIdAsync(Guid id);
        Task<Organization> CreateAsync(Organization org);
        Task<Organization> UpdateAsync(Organization org);
        Task<bool> DeleteAsync(Guid id);
    }
}
