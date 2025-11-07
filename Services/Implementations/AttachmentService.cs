using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Services.Interfaces;

namespace NOTIFICATIONSAPP.Services.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAttachmentRepository _repository;

        public AttachmentService(IAttachmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Attachment>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Attachment?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

        public async Task<Attachment> CreateAsync(Attachment attachment)
        {
            await _repository.AddAsync(attachment);
            await _repository.SaveChangesAsync();
            return attachment;
        }

        public async Task<Attachment> UpdateAsync(Attachment attachment)
        {
            _repository.Update(attachment);
            await _repository.SaveChangesAsync();
            return attachment;
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
