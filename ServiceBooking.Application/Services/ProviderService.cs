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
public class ProviderService : IProviderService
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ProviderService(IProviderRepository providerRepository, IUnitOfWork uof, IMapper mapper)
    {
        _providerRepository = providerRepository;
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<ProviderDto> RegisterProviderAsync(ProviderForRegistrationDto providerRegisterDto)
    {
        var existingProvider = await _providerRepository.GetAsync(p => p.Name == providerRegisterDto.Name);
        if (existingProvider is not null)
        {
            throw new InvalidOperationException("Esse prestador já foi cadastrado");
        }

        var provider = new Provider
        {
            Name = providerRegisterDto.Name,
            Description = providerRegisterDto.Description,
            LogoUrl = providerRegisterDto.LogoUrl
        };

        _providerRepository.Create(provider);
        await _uof.CommitAsync();

        var providerDto = _mapper.Map<ProviderDto>(provider);
        return providerDto;
    }

    public async Task<ProviderDto?> GetAsync(int id)
    {
        var provider = await _providerRepository.GetAsync(p => p.Id == id);
        if (provider is null)
        {
            return null;
        }

        var providerDto = _mapper.Map<ProviderDto>(provider);
        return providerDto;
    }

    public async Task<IEnumerable<ProviderDto>> GetAllAsync()
    {
        var providers = await _providerRepository.GetAllAsync();

        var providersDto = _mapper.Map<IEnumerable<ProviderDto>>(providers);
        return providersDto;
    }
}
