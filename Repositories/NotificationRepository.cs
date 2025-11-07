using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db) => _db = db;

        public async Task<Notification?> GetByIdAsync(Guid id) =>
            await _db.Notifications
                     .Include(n => n.Attachments)
                     .Include(n => n.UserNotifications)
                     .Include(n => n.DeliveryAttempts)
                     .FirstOrDefaultAsync(n => n.Id == id);

        public async Task<IEnumerable<Notification>> GetAllAsync() =>
            await _db.Notifications.ToListAsync();

        public async Task AddAsync(Notification notification) => await _db.Notifications.AddAsync(notification);

        public void Update(Notification notification) => _db.Notifications.Update(notification);

        public void Remove(Notification notification) => _db.Notifications.Remove(notification);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
