using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Application.Services;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Shared.Common;
using System.Text.Json;

namespace ServiceBooking.API.Controllers;
[Route("[controller]")]
[ApiController]
public class ServiceOfferingController : ControllerBase
{
    private readonly IServiceOfferingService _serviceOfferingService;

    public ServiceOfferingController(IServiceOfferingService serviceOfferingService)
    {
        _serviceOfferingService = serviceOfferingService;
    }

    [HttpGet("{id}", Name = "GetServiceOfferingById")]
    public async Task<IActionResult> GetServiceOfferingById(int id)
    {
        var serviceOfferingDto = await _serviceOfferingService.GetServiceAsync(id);

        if (serviceOfferingDto is null)
        {
            return NotFound("Nenhum servidor encontrado");
        }

        return Ok(serviceOfferingDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] QueryParameters queryParameters)
    {
        var pagedResult = await _serviceOfferingService.GetAllServicesAsync(queryParameters);

        // Cria um objeto de metadados da paginação
        var paginationMetadata = new
        {
            pagedResult.TotalCount,
            pagedResult.PageSize,
            pagedResult.PageNumber,
            pagedResult.TotalPages,
            pagedResult.HasNextPage,
            pagedResult.HasPreviousPage
        };

        // Adiciona os metadados serializados ao header da resposta
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(pagedResult.Items);
    }
    [HttpPost]
    public async Task<ActionResult<ServiceOfferingForRegistrationDTO>> RegisterServiceAsync(ServiceOfferingForRegistrationDTO serviceDto)
    {
        try
        {
            var createdService = await _serviceOfferingService.RegisterServiceAsync(serviceDto);
            return CreatedAtAction(nameof(GetServiceOfferingById),
                                    new { id = createdService.Id },
                                    createdService);
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

    [HttpPut("{id}", Name = "UpdateService")]
    //[Authorize]
    public async Task<ActionResult<ServiceOfferingDTO>> UpdateServiceAsync([FromBody]ServiceOfferingForUpdateDTO serviceDto, int id)
    {
        try
        {
            var serviceOfferingDto = await _serviceOfferingService.UpdateServiceOfferingAsync(serviceDto, id);

            if (serviceOfferingDto is null)
            {
                return NotFound($"Nenhum serviço foi encontrado com o ID {id}");
            }

            return Ok(serviceOfferingDto);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocorreu um erro inesperado ao atualizador o serviço");
        }
    }

    [HttpPut("{id}/providers", Name = "UpdateProviderServices")]
    //[Authorize]
    public async Task<ActionResult<ProviderDetailsDto>> UpdateProvidersOfServices([FromBody] ServiceOfferingUpdatesProvidersDTO serviceDto, int id)
    {
        try
        {
            var updatedService = await _serviceOfferingService.UpdateProvidersAsync(serviceDto, id);

            if (updatedService is null)
            {
                return NotFound($"Nenhum serviço foi encontrado com o ID {id}");
            }

            return Ok(updatedService);
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

    [HttpDelete("{id}")]
    //[Authorize]
    public async Task<IActionResult> DeleteServiceOfferingAsync(int id)
    {
        bool result = await _serviceOfferingService.DeleteAsync(id);
        if (!result)
        {
            return NotFound($"Nenhum serviço foi encontrado com o ID {id}");
        }

        return NoContent();
    }
}
