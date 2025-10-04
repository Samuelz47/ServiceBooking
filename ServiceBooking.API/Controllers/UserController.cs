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
    private readonly IUnitOfWork _uof;

    public UserController(IUserService userService, IUnitOfWork uof)
    {
        _userService = userService;
        _uof = uof;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserForRegistrationDto>> RegisterUserAsync(UserForRegistrationDto userDto)
    {
        try
        {
            var createdUser = await _userService.RegisterUserAsync(userDto);
            await _uof.CommitAsync();
            return Ok(new
            {
                Message = "Usuário criado com sucesso",
                UserId = createdUser.Id,
                Email = createdUser.Email
            });
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
}
