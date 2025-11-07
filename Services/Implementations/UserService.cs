using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<User?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<User> CreateAsync(User user)
        {
            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _repository.Update(user);
            await _repository.SaveChangesAsync();
            return user;
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
