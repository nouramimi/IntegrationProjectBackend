using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Repositories.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<Attachment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Attachment>> GetByNotificationIdAsync(Guid notificationId);
        Task AddAsync(Attachment attachment);
        void Update(Attachment attachment);
        void Remove(Attachment attachment);
        Task SaveChangesAsync();
        Task<IEnumerable<Attachment>> GetAllAsync();

    }
}
