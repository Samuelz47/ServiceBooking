using BCrypt.Net;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uof;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository userRepository, IUnitOfWork uof, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _uof = uof;
        _tokenService = tokenService;
    }

    public async Task<string> Login(LoginDTO loginDto)
    {
        if (loginDto is null)
        {
            throw new ArgumentNullException(nameof(loginDto));
        }

        var existingUser = await _userRepository.GetUserByEmailAsync(loginDto.Email);
        if (existingUser is null)
        {
            throw new InvalidOperationException("Email ou senha inválidos");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, existingUser.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        var token = _tokenService.GenerateToken(existingUser);
        return token;
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
            Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password)     //Salvando a senha encriptografada
        };

        await _userRepository.AddUserAsync(newUser);        // Adiciona ao banco
        return newUser;
    }
}
