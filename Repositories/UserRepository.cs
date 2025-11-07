using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users
                .Include(u => u.UserNotifications)
                    .ThenInclude(un => un.Notification)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .Include(u => u.UserNotifications)
                    .ThenInclude(un => un.Notification)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _db.Users.Update(user);
        }

        public void Remove(User user)
        {
            _db.Users.Remove(user);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
