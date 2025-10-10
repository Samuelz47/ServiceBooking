using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
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
    Task<IEnumerable<BookingDTO>> GetBookingsByUserIdAsync(int userId);
}
