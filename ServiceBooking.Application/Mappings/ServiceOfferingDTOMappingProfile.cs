using AutoMapper;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Mappings;
public class ServiceOfferingDTOMappingProfile : Profile
{
    public ServiceOfferingDTOMappingProfile()
    {
        CreateMap<ServiceOfferingForRegistrationDTO, ServiceOffering>();

        CreateMap<ServiceOffering, ServiceOfferingDTO>();
        CreateMap<ServiceOffering, ServiceOfferingDetailsDTO>();
    }
}
