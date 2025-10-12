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
public class ServiceOfferingRepository : Repository<ServiceOffering>, IServiceOfferingRepository
{
    public ServiceOfferingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<ServiceOffering>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.ServicesOfferings.Where(p => ids.Contains(p.Id))
                                               .AsNoTracking()
                                               .ToListAsync();
    }

    public async Task<ServiceOffering?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.ServicesOfferings.Include(s => s.Providers)
                                               .FirstOrDefaultAsync(s => s.Id == id);    
    }
}
