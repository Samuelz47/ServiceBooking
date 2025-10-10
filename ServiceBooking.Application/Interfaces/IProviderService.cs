using ServiceBooking.Application.DTOs;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Interfaces;
public interface IProviderService
{
    Task<ProviderDto> RegisterProviderAsync(ProviderForRegistrationDto ProviderDto);
    Task<ProviderDto?> GetAsync(int id);
    Task<PagedResult<ProviderDto>> GetAllAsync(QueryParameters queryParameters);
}
