using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.DTOs;
public class BookingDTO
{
    public int Id { get; set; }
    public DateTime InitialDate { get; set; }
    public string Status { get; set; }
    public int ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public int ServiceOfferingId { get; set; }
    public string ServiceOfferingName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
}
