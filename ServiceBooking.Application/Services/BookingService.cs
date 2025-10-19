using AutoMapper;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Shared.Common;
using ServiceBooking.Domain.Enums;
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

    public async Task<bool> CancelAsync(int id, int userId)
    {
        var existingBooking = await _bookingRepository.GetByIdAndUserIdAsync(id, userId);

        if (existingBooking is null)
        {
            return false;
        }

        existingBooking.Status = BookingStatus.Cancelled;
        await _uof.CommitAsync();
        return true;
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
        newBooking.FinalDate = dto.InitialDate.AddHours(serviceOffering.TotalHours);

        var conflictingBookings = await _bookingRepository.GetConflictingBookingsAsync(
        provider.Id,
        newBooking.InitialDate,
        newBooking.FinalDate
        );

        if (conflictingBookings.Count >= provider.ConcurrentCapacity)
        {
            throw new InvalidOperationException("Este horário não está disponível para o provedor selecionado.");
        }

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

    public async Task<BookingDTO> UpdateBookingAsync(int id, int userId, BookingForRescheduleDTO dto)
    {
        var booking = await _bookingRepository.GetByIdAndUserIdAsync(id, userId);   //Verificando se o agendamento está no cadastro do usuario
        if (booking is null)
        {
            return null;
        }

        var finalProviderId = dto.ProviderId ?? booking.ProviderId;
        var finalStartTime = dto.InitialDate ?? booking.InitialDate;
        var finalEndTime = finalStartTime.AddHours(booking.ServiceOffering.TotalHours);

        var finalProvider = (finalProviderId == booking.ProviderId) 
                            ? booking.Provider 
                            : await _providerRepository.GetAsync(p => p.Id == finalProviderId);
        if (finalProvider is null)
        {
            throw new InvalidOperationException("Provedor não encontrado.");
        }

        var existingBookings = await _bookingRepository.GetConflictingBookingsAsync(finalProviderId, finalStartTime, finalEndTime, id);
        if (existingBookings.Count >= finalProvider.ConcurrentCapacity)
        {
            throw new InvalidOperationException("Novo Provedor sem horário disponível");
        }

        booking.ProviderId = finalProviderId;
        booking.Provider = finalProvider;
        booking.Status = BookingStatus.Pending;
        booking.InitialDate = finalStartTime;
        booking.FinalDate = finalEndTime;

        _bookingRepository.Update(booking);
        await _uof.CommitAsync();

        var updatedBooking = _mapper.Map<BookingDTO>(booking);
        return updatedBooking;
    }
}
