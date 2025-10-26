using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Application.Services;
using ServiceBooking.Shared.Common;
using System.Security.Claims;
using System.Text.Json;

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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMyBookings([FromQuery] QueryParameters queryParameters)
    {
        // 1. Pega o ID do usuário logado a partir do token JWT de forma segura.
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized("Token inválido ou não contém o ID do usuário.");
        }

        // 2. Chama o serviço, que agora retorna o resultado paginado de DTOs.
        var pagedResult = await _bookingService.GetBookingsByUserIdAsync(userId, queryParameters);

        // 3. Cria o objeto de metadados para ser enviado no cabeçalho.
        var paginationMetadata = new
        {
            pagedResult.TotalCount,
            pagedResult.PageSize,
            pagedResult.PageNumber,
            pagedResult.TotalPages,
            pagedResult.HasNextPage,
            pagedResult.HasPreviousPage
        };

        // 4. Adiciona o cabeçalho X-Pagination à resposta, serializando os metadados como JSON.
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

        // 5. Retorna um 200 OK com apenas a lista de itens da página atual no corpo da resposta.
        return Ok(pagedResult.Items);
    }

    [HttpGet("provider-schedule")]
    [Authorize (Roles = "Provider, Admin")]
    public async Task<IActionResult> GetBookingsByProvider([FromQuery] QueryParameters queryParameters)
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

        var pagedResult = await _bookingService.GetBookingsByProvidersAsync(userId, queryParameters);

        if (pagedResult is null)
        {
            return Forbid("O usuário atual não está associado a um perfil de provedor.");
        }

        var paginationMetadata = new 
        {
            pagedResult.TotalCount,
            pagedResult.PageSize,
            pagedResult.PageNumber,
            pagedResult.TotalPages,
            pagedResult.HasNextPage,
            pagedResult.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(pagedResult.Items);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> RegisterBooking([FromBody] BookingForRegistrationDTO bookingDto)
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

    [HttpPut("{id}", Name = "BookingUpdate")]
    [Authorize(Roles = "User")]

    public async Task<ActionResult<BookingDTO>> UpdateBooking (BookingForRescheduleDTO dto, int id)
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

        var booking = await _bookingService.UpdateBookingAsync(id, userId, dto);
        if (booking == null)
        {
            return NotFound("Usuário não possui agendamento");
        }

        return Ok(booking);
    }

    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "Provider, Admin")]

    public async Task<ActionResult<BookingDTO>> ConfirmBooking(int id)
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

        var updatedBooking = await _bookingService.ConfirmBookingAsync(id, userId);

        if (updatedBooking is null)
        {
            return NotFound("ID do agendamento não encontrado");
        }

        return Ok(updatedBooking);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "User, Provider, Admin")]
    public async Task<IActionResult> CancelBooking (int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        if (userIdClaim is null || roleClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized("Token inválido ou não contém o ID do usuário.");
        }

        bool itsProvider = User.IsInRole("Provider");
        
        bool cancelledBooking = await _bookingService.CancelAsync(id, userId, itsProvider);
        if (!cancelledBooking)
        {
            return NotFound("Agendamento não encontrado ou não pertence a este usuário.");
        }

        return NoContent();
    }
}
