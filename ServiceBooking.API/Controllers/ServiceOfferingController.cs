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
        var createdService = await _serviceOfferingService.RegisterServiceAsync(serviceDto);
        return CreatedAtAction(nameof(GetServiceOfferingById),
                                new { id = createdService.Id },
                                createdService);
    }

    [HttpPut("{id}", Name = "UpdateService")]
    //[Authorize]
    public async Task<ActionResult<ServiceOfferingDTO>> UpdateServiceAsync([FromBody]ServiceOfferingForUpdateDTO serviceDto, int id)
    {
        var serviceOfferingDto = await _serviceOfferingService.UpdateServiceOfferingAsync(serviceDto, id);

        if (serviceOfferingDto is null)
        {
            return NotFound($"Nenhum serviço foi encontrado com o ID {id}");
        }

        return Ok(serviceOfferingDto);
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
