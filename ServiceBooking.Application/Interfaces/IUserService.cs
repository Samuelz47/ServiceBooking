using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Interfaces;
public interface IUserService
{
    Task<User> RegisterUserAsync(UserForRegistrationDto userDto);
    Task<string> Login(LoginDTO loginDto);
}
