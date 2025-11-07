using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;

        public OrganizationService(IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Organization>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Organization?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<Organization> CreateAsync(Organization org)
        {
            await _repository.AddAsync(org);
            await _repository.SaveChangesAsync();
            return org;
        }

        public async Task<Organization> UpdateAsync(Organization org)
        {
            _repository.Update(org);
            await _repository.SaveChangesAsync();
            return org;
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
