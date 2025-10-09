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
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Bookings
                         .Include(b => b.Provider)          // Inclui o Provider relacionado
                         .Include(b => b.ServiceOffering)   // Inclui o ServiceOffering relacionado
                         .Include(b => b.User)              // Inclui o User relacionado
                         .FirstOrDefaultAsync(b => b.Id == id);
    }
}
