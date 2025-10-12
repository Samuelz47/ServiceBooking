using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.DTOs;
public class ServiceOfferingUpdatesProvidersDTO
{
    public List<int> ProvidersIds { get; set; } = new List<int>();
}
