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
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceOfferingRepository _serviceOfferingRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public BookingService(IBookingRepository bookingRepository, IUnitOfWork uof, IMapper mapper, IServiceOfferingRepository serviceOfferingRepository, IProviderRepository providerRepository, IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _uof = uof;
        _mapper = mapper;
        _serviceOfferingRepository = serviceOfferingRepository;
        _providerRepository = providerRepository;
        _userRepository = userRepository;
    }
    public async Task<BookingDTO> CreateBookingAsync(BookingForRegistrationDTO dto, int userId)
    {
        var provider = await _providerRepository.GetAsync(p => p.Id == dto.ProviderId);
        if (provider is null)
        {
            throw new InvalidOperationException("O provedor especificado não existe.");
        }

        var serviceOffering = await _serviceOfferingRepository.GetAsync(s => s.Id == dto.ServiceOfferingId);
        if (serviceOffering is null)
        {
            throw new InvalidOperationException("O serviço especificado não existe.");
        }

        var user = await _userRepository.GetAsync(u => u.Id == userId);
        if (user is null)
        {
            throw new InvalidOperationException("O usuário especificado não existe.");
        }

        var newBooking = new Booking(serviceOffering, provider, user);
        newBooking.InitialDate = dto.InitialDate;

        _bookingRepository.Create(newBooking);
        await _uof.CommitAsync();
        var bookingDto = _mapper.Map<BookingDTO>(newBooking);
        return bookingDto;
    }

    public async Task<BookingDTO?> GetBookingAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);
        if (booking is null)
        {
            return null;
        }

        var bookingDto = _mapper.Map<BookingDTO>(booking);
        return bookingDto;
    }

    public async Task<PagedResult<BookingDTO>> GetBookingsByUserIdAsync(int userId, QueryParameters queryParameters)
    {
        // 1. Pede ao repositório o resultado paginado de ENTIDADES
        var pagedResultFromRepo = await _bookingRepository.GetByUserIdAsync(userId, queryParameters);

        // 2. Usa o AutoMapper para converter a lista de ENTIDADES (Items) em uma lista de DTOs
        var bookingDtos = _mapper.Map<IEnumerable<BookingDTO>>(pagedResultFromRepo.Items);

        // 3. Cria e retorna um NOVO resultado paginado, agora contendo os DTOs,
        //    mas preservando os metadados de paginação (TotalCount, PageNumber, etc.) que vieram do repositório.
        return new PagedResult<BookingDTO>(
            bookingDtos,
            pagedResultFromRepo.TotalCount,
            pagedResultFromRepo.PageNumber,
            pagedResultFromRepo.PageSize);
    }
}
