using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
        var createdProvider = await _providerService.CreateProviderWithUserAsync(providerDto);
        return CreatedAtAction(nameof(GetProviderById), 
                                new { id = createdProvider.Id },
                                createdProvider);
    }

    [HttpPut("{id}", Name = "UpdateProvider")]
    //[Authorize]
    public async Task<ActionResult<ProviderDto>> UpdateProviderAsync([FromBody] ProviderForUpdateDTO providerDto, int id)
    {
        var updatedProvider = await _providerService.UpdateAsync(providerDto, id);

        if (updatedProvider is null)
        {
            return NotFound($"Nenhum provedor foi encontrado com o ID {id}");
        }

        return Ok(updatedProvider);
    }

    [HttpPut("{id}/services", Name = "UpdateServicesProvider" )]
    //[Authorize]
    public async Task<ActionResult<ProviderDetailsDto>> UpdateServicesOfProvider([FromBody] ProviderUpdateServicesDTO providerDto, int id)
    {
        var updatedProvider = await _providerService.UpdateServicesAsync(providerDto, id);

        if(updatedProvider is null)
        {
            return NotFound($"Nenhum provedor foi encontrado com o ID {id}");
        }

        return Ok(updatedProvider);
    }

    [HttpDelete("{id}")]
    //[Authorize]
    public async Task<IActionResult> DeleteProviderAsync(int id)
    {
        bool result = await _providerService.DeleteAsync(id);
        if (!result)
        {
            return NotFound($"Nenhum provedor foi encontrado com o ID {id}");
        }

        return NoContent();
    }
}
