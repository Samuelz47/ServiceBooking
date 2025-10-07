using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Application.Services;

namespace ServiceBooking.API.Controllers;
[Route("[controller]")]
[ApiController]
public class ProviderController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProviderController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet("{id}", Name = "GetProviderById")]
    public async Task<IActionResult> GetProviderById(int id)
    {
        var providerDto = await _providerService.GetAsync(id);

        if (providerDto is null)
        {
            return NotFound("Nenhum provedor encontrado");
        }

        return Ok(providerDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProviderForRegistrationDto>> RegisterProviderAsync(ProviderForRegistrationDto providerDto)
    {
        try
        {
            var createdProvider = await _providerService.RegisterProviderAsync(providerDto);
            return CreatedAtAction(nameof(GetProviderById), 
                                    new { Id = createdProvider.Id },
                                    createdProvider);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Ocorreu um erro inesperado no servidor." });
        }
    }
}
