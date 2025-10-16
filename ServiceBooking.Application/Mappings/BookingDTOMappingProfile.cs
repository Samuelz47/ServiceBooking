using AutoMapper;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Mappings;
public class BookingDTOMappingProfile : Profile
{
    public BookingDTOMappingProfile()
    {
        CreateMap<BookingForRegistrationDTO, Booking>();
        CreateMap<BookingForUpdateDTO, Booking>();

        // Para o Status em bookingDTO (que é string) entender a "tradução" do enum
        CreateMap<Booking, BookingDTO>().ForMember(dest => dest.ProviderName, opt => opt
                                        .MapFrom(src => src.Provider.Name))
                                        .ForMember(dest => dest.ServiceOfferingName, opt => opt
                                        .MapFrom(src => src.ServiceOffering.Name))
                                        .ForMember(dest => dest.UserName, opt => opt
                                        .MapFrom(src => src.User.Name))
                                        .ForMember(dto => dto.Status,opt => opt 
                                        .MapFrom(entidade => entidade.Status.ToString())); 
    }
}
