using NOTIFICATIONSAPP.Models;

namespace NOTIFICATIONSAPP.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<IEnumerable<Attachment>> GetAllAsync();
        Task<Attachment?> GetByIdAsync(Guid id);
        Task<Attachment> CreateAsync(Attachment attachment);
        Task<Attachment> UpdateAsync(Attachment attachment);
        Task<bool> DeleteAsync(Guid id);
    }
}
