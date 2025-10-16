using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Interfaces;
public interface IBookingService
{
    Task<BookingDTO> CreateBookingAsync(BookingForRegistrationDTO dto, int id);
    Task<BookingDTO> GetBookingAsync(int id);
    Task<PagedResult<BookingDTO>> GetBookingsByUserIdAsync(int userId, QueryParameters queryParameters);
    Task<bool> CancelAsync(int id, int userId);
}
