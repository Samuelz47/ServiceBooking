using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.DTOs;
public class BookingForUpdateDTO
{
    public int ProviderId { get; set; }
    public DateTime InitialDate { get; set; }
}
