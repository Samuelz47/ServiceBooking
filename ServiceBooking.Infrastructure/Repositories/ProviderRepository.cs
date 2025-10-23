using Microsoft.EntityFrameworkCore;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Infrastructure.Repositories;
public class ProviderRepository : Repository<Provider>, IProviderRepository
{
    public ProviderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Provider>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Providers.Where(p => ids.Contains(p.Id))
                                       .AsNoTracking()
                                       .ToListAsync();
    }

    public async Task<Provider?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Providers.Include(s => s.Services)
                                       .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Provider?> GetByUserId(int userId)
    {
        return await _context.Providers.Include(u => u.User)
                                       .FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
