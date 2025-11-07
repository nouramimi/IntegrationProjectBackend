using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        void Update(User user);
        void Remove(User user);
        Task SaveChangesAsync();
    }
}
