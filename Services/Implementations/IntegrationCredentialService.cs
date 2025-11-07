using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class IntegrationCredentialService : IIntegrationCredentialService
    {
        private readonly IIntegrationCredentialRepository _repository;

        public IntegrationCredentialService(IIntegrationCredentialRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<IntegrationCredential>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<IntegrationCredential?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<IntegrationCredential> CreateAsync(IntegrationCredential credential)
        {
            await _repository.AddAsync(credential);
            await _repository.SaveChangesAsync();
            return credential;
        }

        public async Task<IntegrationCredential> UpdateAsync(IntegrationCredential credential)
        {
            _repository.Update(credential);
            await _repository.SaveChangesAsync();
            return credential;
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
