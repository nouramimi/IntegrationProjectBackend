using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private readonly AppDbContext _db;

        public UserNotificationRepository(AppDbContext db) => _db = db;

        public async Task<UserNotification?> GetByIdAsync(Guid id) =>
            await _db.UserNotifications
                     .Include(un => un.Notification)
                     .Include(un => un.User)
                     .FirstOrDefaultAsync(un => un.Id == id);

        public async Task<IEnumerable<UserNotification>> GetByUserIdAsync(Guid userId) =>
            await _db.UserNotifications
                     .Where(un => un.UserId == userId)
                     .Include(un => un.Notification)
                     .ToListAsync();

        public async Task AddAsync(UserNotification un) => await _db.UserNotifications.AddAsync(un);

        public void Update(UserNotification un) => _db.UserNotifications.Update(un);

        public void Remove(UserNotification un) => _db.UserNotifications.Remove(un);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IEnumerable<UserNotification>> GetAllAsync() =>
            await _db.UserNotifications.ToListAsync();

    }
}
