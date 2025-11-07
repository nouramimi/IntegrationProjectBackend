using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories 
{
    public class IntegrationRepository : IIntegrationRepository
    {
        private readonly AppDbContext _db;

        public IntegrationRepository(AppDbContext db) => _db = db;

        public async Task<Integration?> GetByIdAsync(Guid id) =>
            await _db.Integrations
                     .Include(i => i.Credentials)
                     .Include(i => i.Channels)
                     .Include(i => i.Notifications)
                     .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<Integration>> GetAllAsync() =>
            await _db.Integrations.ToListAsync();

        public async Task AddAsync(Integration integration) =>
            await _db.Integrations.AddAsync(integration);

        public void Update(Integration integration) => _db.Integrations.Update(integration);

        public void Remove(Integration integration) => _db.Integrations.Remove(integration);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
