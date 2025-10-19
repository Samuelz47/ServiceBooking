using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.DTOs;
public class ServiceOfferingForRegistrationDTO
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int? TotalHours { get; set; }
}
