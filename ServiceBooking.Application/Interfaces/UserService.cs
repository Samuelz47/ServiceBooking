using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Interfaces;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> RegisterUserAsync(UserForRegistrationDto userDto)
    {
        if (userDto is null)        // Verifica se o Dto é nulo (programação defensiva)
        {
            throw new ArgumentNullException(nameof(userDto));
        }

        var existingUser = await _userRepository.GetUserByEmailAsync(userDto.Email);    // Verifica se o email já existe na nossa base
        if (existingUser is not null)       // Se existir lançamos a exceção
        {
            throw new InvalidOperationException("Este email já foi cadastrado");
        }

        var newUser = new User              // Cria usuário com os dados do Dto
        {
            Name = userDto.Name,
            Email = userDto.Email,
            Password = userDto.Password,
        };

        await _userRepository.AddUserAsync(newUser);        // Adiciona ao banco
        return newUser;
    }
}
