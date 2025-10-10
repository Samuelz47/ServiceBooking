using Microsoft.EntityFrameworkCore;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Infrastructure.Context;
using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Infrastructure.Repositories;
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Bookings.Include(b => b.Provider)          // Inclui o Provider relacionado
                                      .Include(b => b.ServiceOffering)   // Inclui o ServiceOffering relacionado
                                      .Include(b => b.User)              // Inclui o User relacionado
                                      .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<PagedResult<Booking>> GetByUserIdAsync(int userId, QueryParameters queryParameters)
    {
        var query = _context.Bookings.Where(b => b.UserId == userId);

        var totalCount = await query.CountAsync();
        
        var items = await query.Include(b => b.Provider)
                               .Include(b => b.ServiceOffering)
                               .Include(b => b.User) 
                               .AsNoTracking() // Boa prática para consultas de leitura.
                               .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                               .Take(queryParameters.PageSize)
                               .ToListAsync();

        return new PagedResult<Booking>(items, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
    }
}
