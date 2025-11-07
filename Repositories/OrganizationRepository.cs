using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Models;
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Data; 

namespace NOTIFICATIONSAPP.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _db;

        public OrganizationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Organization?> GetByIdAsync(Guid id)
        {
            return await _db.Organizations.FindAsync(id);
        }

        public async Task<IEnumerable<Organization>> GetAllAsync()
        {
            return await _db.Organizations.ToListAsync();
        }

        public async Task AddAsync(Organization org)
        {
            await _db.Organizations.AddAsync(org);
        }

        public void Update(Organization org)
        {
            _db.Organizations.Update(org);
        }

        public void Remove(Organization org)
        {
            _db.Organizations.Remove(org);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
