using AutoMapper;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Services;
public class ServiceOfferingService : IServiceOfferingService
{
    private readonly IServiceOfferingRepository _serviceOfferingRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uof;

    public ServiceOfferingService(IServiceOfferingRepository serviceOfferingRepository, IMapper mapper, IUnitOfWork uof)
    {
        _serviceOfferingRepository = serviceOfferingRepository;
        _mapper = mapper;
        _uof = uof;
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

    public async Task<IEnumerable<ServiceOfferingDTO>> GetAllServicesAsync()
    {
        var serviceOffering = await _serviceOfferingRepository.GetAllAsync();

        var serviceOfferingDto = _mapper.Map<IEnumerable<ServiceOfferingDTO>>(serviceOffering);
        return serviceOfferingDto;
    }
}
