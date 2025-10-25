using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using System.Data;

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
    [Authorize(Roles = "Admin")]
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
    public async Task<ActionResult> RegisterUserAsync(UserForRegistrationDto userDto)
    {
        var createdUser = await _userService.RegisterUserAsync(userDto);
        return CreatedAtAction(nameof(GetUser),
                                new { id = createdUser.Id },
                                createdUser);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        var token = await _userService.Login(loginDto);
        return Ok(new { Token = token });
    }
}
