using AutoMapper;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Services;
public class ServiceOfferingService : IServiceOfferingService
{
    private readonly IServiceOfferingRepository _serviceOfferingRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uof;

    public ServiceOfferingService(IServiceOfferingRepository serviceOfferingRepository, IMapper mapper, IUnitOfWork uof, IProviderRepository providerRepository)
    {
        _serviceOfferingRepository = serviceOfferingRepository;
        _mapper = mapper;
        _uof = uof;
        _providerRepository = providerRepository;
    }

    public async Task<ServiceOfferingDetailsDTO?> GetServiceAsync(int id)
    {
        var serviceOffering = await _serviceOfferingRepository.GetAsync(s => s.Id == id);
        if (serviceOffering is null)
        {
            return null;
        }

        var serviceOfferingDTO = _mapper.Map<ServiceOfferingDetailsDTO>(serviceOffering);
        return serviceOfferingDTO;
    }

    public async Task<ServiceOfferingDTO> RegisterServiceAsync(ServiceOfferingForRegistrationDTO serviceOfferingRegDTO)
    {
        var existingServiceOffering = await _serviceOfferingRepository.GetAsync(s => s.Name == serviceOfferingRegDTO.Name);
        if (existingServiceOffering is not null)
        {
            throw new InvalidOperationException("Esse serviço já foi cadastrado");
        }

        var newServiceOffering = _mapper.Map<ServiceOffering>(serviceOfferingRegDTO);
        _serviceOfferingRepository.Create(newServiceOffering);
        await _uof.CommitAsync();

        var serviceOfferingDTO = _mapper.Map<ServiceOfferingDTO>(newServiceOffering);
        return serviceOfferingDTO;
    }

    public async Task<PagedResult<ServiceOfferingDTO>> GetAllServicesAsync(QueryParameters queryParameters)
    {
        var pagedResult = await _serviceOfferingRepository.GetAllAsync(queryParameters);

        var serviceOfferingDto = _mapper.Map<IEnumerable<ServiceOfferingDTO>>(pagedResult.Items);
        return new PagedResult<ServiceOfferingDTO>(serviceOfferingDto, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
    }

    public async Task<ServiceOfferingDTO?> UpdateServiceOfferingAsync(ServiceOfferingForUpdateDTO serviceDto, int id)
    {
        var service = await _serviceOfferingRepository.GetAsync(p => p.Id == id);
        if (service is null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(serviceDto.Name))
        {
            service.Name = serviceDto.Name;
        }

        if (!string.IsNullOrEmpty(serviceDto.Description))
        {
            service.Description = serviceDto.Description;
        }

        _serviceOfferingRepository.Update(service);
        await _uof.CommitAsync();

        var updatedService = _mapper.Map<ServiceOfferingDTO>(service);
        return updatedService;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var service = await _serviceOfferingRepository.GetAsync(p => p.Id == id);
        if (service is null)
        {
            return false;
        }

        _serviceOfferingRepository.Delete(service);
        await _uof.CommitAsync();
        return true;
    }

    public async Task<ServiceOfferingDetailsDTO?> UpdateProvidersAsync(ServiceOfferingUpdatesProvidersDTO serviceUpdate, int id)
    {
        var service = await _serviceOfferingRepository.GetByIdWithDetailsAsync(id);     // Pegamos o servico atraves do ID
        if (service is null)
        {
            return null;
        }

        if(serviceUpdate.ProvidersIds is null)                                          // Verificando se os ids para provedores passados no DTO são nulos
        {
            throw new ArgumentException("A lista de IDs de provedor não pode ser nula");
        }

        var providersFromDb = await _providerRepository.GetByIdsAsync(serviceUpdate.ProvidersIds);      // Transformando a lista de IDs recebida em uma lista de Entidades de provedores

        if (providersFromDb.Count != serviceUpdate.ProvidersIds.Count)                                  // Verificação se houve alguma falha na hora da transformação
        {
            throw new InvalidOperationException("Um ou mais IDs de provedor enviados são inválidos.");
        }
        
        service.Providers.Clear();                                                      // Limpa a lista de provedores dentro de serviços
        foreach (var provider in providersFromDb)
        {
            service.Providers.Add(provider);                                            // Preenche ela com as entidades recebidas
        }
        
        var serviceDto = _mapper.Map<ServiceOfferingDetailsDTO>(service);
        await _uof.CommitAsync();
        return serviceDto;
    }
}
