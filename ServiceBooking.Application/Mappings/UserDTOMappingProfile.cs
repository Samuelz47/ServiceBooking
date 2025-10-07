using AutoMapper;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Mappings;
public class UserDTOMappingProfile : Profile
{
    public UserDTOMappingProfile()
    {
        CreateMap<UserForRegistrationDto, User>();
        
        CreateMap<User, UserDTO>();
    }
}
