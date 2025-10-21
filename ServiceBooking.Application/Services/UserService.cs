using AutoMapper;
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
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, ITokenService tokenService, IUnitOfWork uof, IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _uof = uof;
        _mapper = mapper;
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

    public async Task<UserDTO> RegisterUserAsync(UserForRegistrationDto userRegisterDto)
    {
        if (userRegisterDto is null)        // Verifica se o Dto é nulo (programação defensiva)
        {
            throw new ArgumentNullException(nameof(userRegisterDto));
        }

        var existingUser = await _userRepository.GetUserByEmailAsync(userRegisterDto.Email);    // Verifica se o email já existe na nossa base
        if (existingUser is not null)       // Se existir lançamos a exceção
        {
            throw new InvalidOperationException("Este email já foi cadastrado");
        }

        var newUser = _mapper.Map<User>(userRegisterDto);                               // Cria usuário com os dados do Dto
        newUser.Password = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);    //Salvando a senha encriptografada
        newUser.Role = "Client";

        await _userRepository.AddUserAsync(newUser);                                    // Adiciona ao banco
        await _uof.CommitAsync();
        var userDto = _mapper.Map<UserDTO>(newUser);
        return userDto;
    }

    public async Task<UserDTO?> GetAsync(int id)
    {
        var user = await _userRepository.GetAsync(u =>  u.Id == id);
        if (user is null)
        {
            return null;
        }

        var userDto = _mapper.Map<UserDTO>(user);
        return userDto;
    }
}
