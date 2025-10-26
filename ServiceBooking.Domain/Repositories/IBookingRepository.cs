using ServiceBooking.Domain.Entities;
using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Repositories;
public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetByIdWithDetailsAsync(int id);
    Task<PagedResult<Booking>> GetByUserIdAsync(int userId, QueryParameters queryParameters);
    Task<Booking?> GetByIdAndUserIdAsync(int bookingId, int userId);
    Task<List<Booking>> GetConflictingBookingsAsync(int providerId, DateTime newStartTime, DateTime newEndTime, int? bookingIdToExclude = null);
    Task<PagedResult<Booking>> GetBookingsByProviderIdAsync(int providerId, QueryParameters queryParameters);
    Task<Booking?> GetByIdAndProviderIdAsync(int id, int providerId);
}
