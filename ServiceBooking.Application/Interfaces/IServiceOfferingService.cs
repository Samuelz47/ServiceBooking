using ServiceBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Interfaces;
public interface IServiceOfferingService
{
    Task<ServiceOfferingDTO> RegisterServiceAsync(ServiceOfferingForRegistrationDTO serviceOfferingRegDTO);
    Task<ServiceOfferingDetailsDTO> GetServiceAsync(int id);
    Task<IEnumerable<ServiceOfferingDTO>> GetAllServicesAsync();
}
