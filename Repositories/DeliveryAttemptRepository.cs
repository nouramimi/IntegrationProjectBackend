using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories
{
    public class DeliveryAttemptRepository : IDeliveryAttemptRepository
    {
        private readonly AppDbContext _db;

        public DeliveryAttemptRepository(AppDbContext db) => _db = db;

        public async Task<DeliveryAttempt?> GetByIdAsync(Guid id) =>
            await _db.DeliveryAttempts.FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<DeliveryAttempt>> GetByNotificationIdAsync(Guid notificationId) =>
            await _db.DeliveryAttempts.Where(a => a.NotificationId == notificationId).ToListAsync();

        public async Task AddAsync(DeliveryAttempt attempt) => await _db.DeliveryAttempts.AddAsync(attempt);

        public void Update(DeliveryAttempt attempt) => _db.DeliveryAttempts.Update(attempt);

        public void Remove(DeliveryAttempt attempt) => _db.DeliveryAttempts.Remove(attempt);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IEnumerable<DeliveryAttempt>> GetAllAsync() =>
            await _db.DeliveryAttempts.ToListAsync();

    }
}
