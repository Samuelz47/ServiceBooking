using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.DTOs;
public class BookingForRegistrationDTO
{
    [Required]
    public int ServiceOfferingId { get; set; }
    [Required]
    public int ProviderId { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public DateTime InitialDate { get; set; }
}
