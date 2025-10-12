using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Application.Services;
using ServiceBooking.Shared.Common;
using System.Text.Json;

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

    [HttpGet]
    public async Task<IActionResult> GetAllProvidersAsync([FromQuery] QueryParameters queryParameters)
    {
        var pagedResult = await _providerService.GetAllAsync(queryParameters);

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
    public async Task<ActionResult<ProviderForRegistrationDto>> RegisterProviderAsync(ProviderForRegistrationDto providerDto)
    {
        try
        {
            var createdProvider = await _providerService.RegisterProviderAsync(providerDto);
            return CreatedAtAction(nameof(GetProviderById), 
                                    new { id = createdProvider.Id },
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

    [HttpPut("{id}", Name = "UpdateProvider")]
    //[Authorize]
    public async Task<ActionResult<ProviderDto>> UpdateProviderAsync([FromBody] ProviderForUpdateDTO providerDto, int id)
    {
        try
        {
            var updatedProvider = await _providerService.UpdateAsync(providerDto, id);

            if (updatedProvider is null)
            {
                return NotFound($"Nenhum serviço foi encontrado com o ID {id}");
            }

            return Ok(updatedProvider);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocorreu um erro inesperado ao atualizador o serviço");
        }
    }
}
