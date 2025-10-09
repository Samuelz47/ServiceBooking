using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Repositories;
public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetByIdWithDetailsAsync(int id);
}
