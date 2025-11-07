using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories
{
    public class IntegrationChannelRepository : IIntegrationChannelRepository
    {
        private readonly AppDbContext _db;

        public IntegrationChannelRepository(AppDbContext db) => _db = db;

        public async Task<IntegrationChannel?> GetByIdAsync(Guid id) =>
            await _db.IntegrationChannels
                     .Include(c => c.Notifications)
                     .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<IntegrationChannel>> GetByIntegrationIdAsync(Guid integrationId) =>
            await _db.IntegrationChannels.Where(c => c.IntegrationId == integrationId).ToListAsync();

        public async Task AddAsync(IntegrationChannel channel) => await _db.IntegrationChannels.AddAsync(channel);

        public void Update(IntegrationChannel channel) => _db.IntegrationChannels.Update(channel);

        public void Remove(IntegrationChannel channel) => _db.IntegrationChannels.Remove(channel);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IEnumerable<IntegrationChannel>> GetAllAsync() =>
            await _db.IntegrationChannels.ToListAsync();

    }
}
