using AutoMapper;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Mappings;
public class ProviderDTOMappingProfile : Profile
{
    public ProviderDTOMappingProfile()
    {
        // Mapeamento do DTO de ENTRADA (Criação) para a Entidade
        CreateMap<ProviderForRegistrationDto, Provider>(); 

        // Mapeamentos da Entidade para os DTOs de SAÍDA (Resposta)
        CreateMap<Provider, ProviderDto>();
        CreateMap<Provider, ProviderDetailsDto>();
    }
}
