using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories
{
    public class IntegrationCredentialRepository : IIntegrationCredentialRepository
    {
        private readonly AppDbContext _db;

        public IntegrationCredentialRepository(AppDbContext db) => _db = db;

        public async Task<IntegrationCredential?> GetByIdAsync(Guid id) =>
            await _db.IntegrationCredentials.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<IntegrationCredential>> GetByIntegrationIdAsync(Guid integrationId) =>
            await _db.IntegrationCredentials.Where(c => c.IntegrationId == integrationId).ToListAsync();

        public async Task AddAsync(IntegrationCredential credential) =>
            await _db.IntegrationCredentials.AddAsync(credential);

        public void Update(IntegrationCredential credential) => _db.IntegrationCredentials.Update(credential);

        public void Remove(IntegrationCredential credential) => _db.IntegrationCredentials.Remove(credential);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IEnumerable<IntegrationCredential>> GetAllAsync() =>
            await _db.IntegrationCredentials.ToListAsync();

    }
}
