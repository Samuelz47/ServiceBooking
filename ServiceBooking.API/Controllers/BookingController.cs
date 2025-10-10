using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using System.Security.Claims;

namespace ServiceBooking.API.Controllers;
[Route("[controller]")]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ITokenService _tokenService;

    public BookingController(IBookingService bookingService, ITokenService tokenService)
    {
        _bookingService = bookingService;
        _tokenService = tokenService;
    }

    [HttpGet("{id}", Name = "GetBookingById")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var existingBooking = await _bookingService.GetBookingAsync(id);
        if (existingBooking is null)
        {
            return NotFound("Nenhum agendamento encontrado");
        }
        return Ok(existingBooking);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BookingDTO>>> GetAllBookingsById()
    {
        // 1. Acessa as 'claims' do usuário autenticado pelo token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        // 2. Validação para garantir que o token contém o ID do usuário
        if (userIdClaim is null)
        {
            // Isso não deveria acontecer se o [Authorize] estiver funcionando, mas é uma boa defesa
            return Unauthorized("Token inválido ou não contém o ID do usuário.");
        }

        // 3. Converte o valor do ID (que é uma string) para inteiro
        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized("ID do usuário no token está em um formato inválido.");
        }

        var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
        // Não retornamos os 404 para lista vazia pois fica melhor pro front
        return Ok(bookings);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RegisterBooking([FromBody] BookingForRegistrationDTO bookingDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                return Unauthorized("Token inválido ou não contém o ID do usuário.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("ID do usuário no token está em um formato inválido.");
            }

            var createdBooking = await _bookingService.CreateBookingAsync(bookingDto, userId);
            return CreatedAtAction(nameof(GetBookingById),
                                    new { id = createdBooking.Id },
                                    createdBooking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocorreu um erro inesperado ao processar o agendamento.");
        }
    }
}
