using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data;

namespace NOTIFICATIONSAPP.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly AppDbContext _db;

        public AttachmentRepository(AppDbContext db) => _db = db;

        public async Task<Attachment?> GetByIdAsync(Guid id) =>
            await _db.Attachments.FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Attachment>> GetByNotificationIdAsync(Guid notificationId) =>
            await _db.Attachments.Where(a => a.NotificationId == notificationId).ToListAsync();

        public async Task AddAsync(Attachment attachment) => await _db.Attachments.AddAsync(attachment);

        public void Update(Attachment attachment) => _db.Attachments.Update(attachment);

        public void Remove(Attachment attachment) => _db.Attachments.Remove(attachment);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IEnumerable<Attachment>> GetAllAsync() =>
            await _db.Attachments.ToListAsync();

    }
}
