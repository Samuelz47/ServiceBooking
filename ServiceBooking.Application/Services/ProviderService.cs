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
public class ProviderService : IProviderService
{
    private readonly IProviderRepository _providerRepository;
    private readonly IServiceOfferingRepository _serviceOfferingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ProviderService(IProviderRepository providerRepository, IUnitOfWork uof, IMapper mapper, IServiceOfferingRepository serviceOfferingRepository, IUserRepository userRepository)
    {
        _providerRepository = providerRepository;
        _uof = uof;
        _mapper = mapper;
        _serviceOfferingRepository = serviceOfferingRepository;
        _userRepository = userRepository;
    }

    public async Task<ProviderDetailsDto?> GetAsync(int id)
    {
        var provider = await _providerRepository.GetAsync(p => p.Id == id);
        if (provider is null)
        {
            return null;
        }

        var providerDto = _mapper.Map<ProviderDetailsDto>(provider);
        return providerDto;
    }

    public async Task<PagedResult<ProviderDto>> GetAllAsync(QueryParameters queryParameters)
    {
        var pagedResult = await _providerRepository.GetAllAsync(queryParameters);

        var providersDto = _mapper.Map<IEnumerable<ProviderDto>>(pagedResult.Items);
        return new PagedResult<ProviderDto>(providersDto, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
    }

    public async Task<ProviderDto?> UpdateAsync(ProviderForUpdateDTO providerDto, int id)
    {
        var provider = await _providerRepository.GetAsync(p => p.Id == id);
        if (provider is null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(providerDto.Name))
        {
            provider.Name = providerDto.Name;
        }

        if (!string.IsNullOrEmpty(providerDto.Description))
        {
            provider.Description = providerDto.Description;
        }

        if (!string.IsNullOrEmpty(providerDto.LogoUrl))
        {
            provider.LogoUrl = providerDto.LogoUrl;
        }

        if (providerDto.ConcurrentCapacity.HasValue)
        {
            provider.ConcurrentCapacity = providerDto.ConcurrentCapacity.Value;
        }

        _providerRepository.Update(provider);
        await _uof.CommitAsync();

        var updatedProvider = _mapper.Map<ProviderDto>(provider);
        return updatedProvider;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var provider = await _providerRepository.GetAsync(p => p.Id == id);
        if (provider is null)
        {
            return false;
        }

        _providerRepository.Delete(provider);
        await _uof.CommitAsync();
        return true;
    }

    public async Task<ProviderDetailsDto> UpdateServicesAsync(ProviderUpdateServicesDTO providerUpdate, int id)
    {
        var provider = await _providerRepository.GetByIdWithDetailsAsync(id);  
        if (provider is null)
        {
            return null;
        }

        if (providerUpdate.ServicesIds is null)                                         
        {
            throw new ArgumentException("A lista de IDs de serviço não pode ser nula");
        }

        var servicesFromDb = await _serviceOfferingRepository.GetByIdsAsync(providerUpdate.ServicesIds);      

        if (servicesFromDb.Count != providerUpdate.ServicesIds.Count)                                
        {
            throw new InvalidOperationException("Um ou mais IDs de provedor enviados são inválidos.");
        }

        provider.Services.Clear();                                                   
        foreach (var service in servicesFromDb)
        {
            provider.Services.Add(service);                                            
        }

        var providerDto = _mapper.Map<ProviderDetailsDto>(provider);
        await _uof.CommitAsync();
        return providerDto;
    }

    public async Task<ProviderDto> CreateProviderWithUserAsync(ProviderForRegistrationDto dto)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("O email fornecido já está em uso.");
        }

        var newUser = new User
        {
            Name = dto.UserName,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Provider"
        };
        _userRepository.Create(newUser);

        var newProvider = new Provider
        {
            Name = dto.Name,
            Description = dto.Description,
            ConcurrentCapacity = dto.ConcurrentCapacity.Value,
            User = newUser
        };
        _providerRepository.Create(newProvider);

        await _uof.CommitAsync();

        return _mapper.Map<ProviderDto>(newProvider);
    }
}
