using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;

namespace ServiceBooking.API.Controllers;
[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<IActionResult> GetUser(int id)
    {
        var userDto = await _userService.GetAsync(id);

        if (userDto is null)
        {
            return NotFound("Nenhum usuário encontrado");
        }

        return Ok(userDto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> RegisterUserAsync(UserForRegistrationDto userDto)
    {
        try
        {
            var createdUser = await _userService.RegisterUserAsync(userDto);
            return CreatedAtAction(nameof(GetUser),
                                    new { Id = createdUser.Id },
                                    createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);      // Captura o erro existente do email.
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Ocorreu um erro inesperado no servidor." });      // Captura qualquer outro erro inesperado
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        try
        {
            var token = await _userService.Login(loginDto);
            return Ok(new { Token = token });
        }
        catch (InvalidOperationException)       // Erro para email não encontrado
        {
            return Unauthorized("Email ou senha inválida");
        }
        catch (UnauthorizedAccessException)     // Erro para senha incorreta
        {
            return Unauthorized("Email ou senha inválida");
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Ocorreu um erro inesperado no servidor." });      // Captura qualquer outro erro inesperado
        }
    }
}
