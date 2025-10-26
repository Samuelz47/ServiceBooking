using AutoMapper;
using Moq;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Services;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Enums;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Shared.Common;
using System.Linq.Expressions;

namespace ServiceBooking.Application.Tests.ServiceUnitTests;

// Documentação: Esta classe testa o BookingService
public class BookingServiceTests
{
    // Mocks para todas as dependências injetadas no BookingService
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IServiceOfferingRepository> _mockServiceRepo;
    private readonly Mock<IProviderRepository> _mockProviderRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IUnitOfWork> _mockUof;
    private readonly Mock<IMapper> _mockMapper;

    // A instância real do serviço que estamos testando
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        // 1. Inicializa todos os mocks no construtor
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockServiceRepo = new Mock<IServiceOfferingRepository>();
        _mockProviderRepo = new Mock<IProviderRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockUof = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        // 2. Cria a instância do serviço, injetando os mocks
        _bookingService = new BookingService(
            _mockBookingRepo.Object,
            _mockUof.Object,
            _mockMapper.Object,
            _mockServiceRepo.Object,
            _mockProviderRepo.Object,
            _mockUserRepo.Object
        );
    }

    #region CreateBookingAsync Tests

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateBooking_WhenSlotIsAvailable()
    {
        // Arrange (Preparar)
        var dto = new BookingForRegistrationDTO
        {
            ProviderId = 1,
            ServiceOfferingId = 1,
            InitialDate = DateTime.UtcNow.AddHours(1)
        };
        var userId = 1;

        var provider = new Provider { Id = 1, ConcurrentCapacity = 1, Name = "Provider Teste" };
        var service = new ServiceOffering { Id = 1, TotalHours = 2, Name = "Serviço Teste" };
        var user = new User { Id = 1, Name = "User Teste" };

        // Configura os mocks para encontrar as entidades
        _mockProviderRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync(provider);
        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>())).ReturnsAsync(service);
        _mockUserRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);

        // Configura o mock de conflito para retornar uma LISTA VAZIA (sem conflitos)
        _mockBookingRepo.Setup(repo => repo.GetConflictingBookingsAsync(
            It.IsAny<int>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            null
        )).ReturnsAsync(new List<Booking>());

        // Configura o mapper
        var expectedDto = new BookingDTO { Id = 1, ProviderName = "Provider Teste" };
        _mockMapper.Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>())).Returns(expectedDto);

        // Act (Agir)
        var result = await _bookingService.CreateBookingAsync(dto, userId);

        // Assert (Verificar)
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        _mockBookingRepo.Verify(repo => repo.Create(It.IsAny<Booking>()), Times.Once); // Verifica se o Create foi chamado
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once); // Verifica se o Commit foi chamado
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowInvalidOperation_WhenProviderNotFound()
    {
        // Arrange
        var dto = new BookingForRegistrationDTO();
        _mockProviderRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync((Provider?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookingService.CreateBookingAsync(dto, 1)
        );
        Assert.Equal("O provedor especificado não existe.", exception.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowInvalidOperation_WhenSlotIsFull()
    {
        // Arrange
        var dto = new BookingForRegistrationDTO { ProviderId = 1 };
        var provider = new Provider { Id = 1, ConcurrentCapacity = 1 }; // Capacidade de 1
        var service = new ServiceOffering { Id = 1, TotalHours = 1 };
        var user = new User { Id = 1 };

        _mockProviderRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync(provider);
        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>())).ReturnsAsync(service);
        _mockUserRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);

        // Configura o mock de conflito para retornar UM AGENDAMENTO (atingindo a capacidade de 1)
        var existingBooking = new Booking(service, provider, user); // Usa o construtor correto
        _mockBookingRepo.Setup(repo => repo.GetConflictingBookingsAsync(
            It.IsAny<int>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            null
        )).ReturnsAsync(new List<Booking> { existingBooking });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookingService.CreateBookingAsync(dto, 1)
        );
        Assert.Equal("Este horário não está disponível para o provedor selecionado.", exception.Message);
    }

    #endregion

    #region CancelAsync Tests

    [Fact]
    public async Task CancelAsync_ShouldCancelBooking_WhenCalledByUser()
    {
        // Arrange
        var bookingId = 1;
        var userId = 1;
        var service = new ServiceOffering { Id = 1, TotalHours = 1 };
        var provider = new Provider { Id = 1 };
        var user = new User { Id = 1 };
        var booking = new Booking(service, provider, user) { Id = bookingId, UserId = userId, Status = BookingStatus.Pending };

        _mockBookingRepo.Setup(repo => repo.GetByIdAndUserIdAsync(bookingId, userId)).ReturnsAsync(booking);

        // Act
        var result = await _bookingService.CancelAsync(bookingId, userId, false);

        // Assert
        Assert.True(result);
        Assert.Equal(BookingStatus.Cancelled, booking.Status); // Verifica se o status foi alterado
        _mockBookingRepo.Verify(repo => repo.Update(booking), Times.Once);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelBooking_WhenCalledByProvider()
    {
        // Arrange
        var bookingId = 1;
        var providerUserId = 10;
        var providerId = 5;
        var provider = new Provider { Id = providerId, UserId = providerUserId };
        var service = new ServiceOffering { Id = 1, TotalHours = 1 };
        var user = new User { Id = 1 };
        var booking = new Booking(service, provider, user) { Id = bookingId, ProviderId = providerId, Status = BookingStatus.Pending };

        _mockProviderRepo.Setup(repo => repo.GetByUserIdAsync(providerUserId)).ReturnsAsync(provider);
        _mockBookingRepo.Setup(repo => repo.GetByIdAndProviderIdAsync(bookingId, providerId)).ReturnsAsync(booking);

        // Act
        var result = await _bookingService.CancelAsync(bookingId, providerUserId, true); // itsProvider = true

        // Assert
        Assert.True(result);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        _mockBookingRepo.Verify(repo => repo.Update(booking), Times.Once);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenBookingNotFound()
    {
        // Arrange
        _mockBookingRepo.Setup(repo => repo.GetByIdAndUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((Booking?)null);

        // Act
        var result = await _bookingService.CancelAsync(99, 1, false);

        // Assert
        Assert.False(result);
        _mockBookingRepo.Verify(repo => repo.Update(It.IsAny<Booking>()), Times.Never);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateBookingAsync Tests

    [Fact]
    public async Task UpdateBookingAsync_ShouldUpdateBooking_WhenSlotIsAvailable()
    {
        // Arrange
        var bookingId = 1;
        var userId = 1;
        var dto = new BookingForRescheduleDTO { InitialDate = DateTime.UtcNow.AddDays(1) };

        var service = new ServiceOffering { Id = 1, TotalHours = 1 };
        var provider = new Provider { Id = 1, ConcurrentCapacity = 1 };
        var user = new User { Id = 1 };
        var booking = new Booking(service, provider, user)
        {
            Id = bookingId,
            UserId = userId,
            ProviderId = 1,
            Status = BookingStatus.Confirmed, // Testa se o status é resetado para Pending
            InitialDate = DateTime.UtcNow.AddHours(2)
        };

        _mockBookingRepo.Setup(repo => repo.GetByIdAndUserIdAsync(bookingId, userId)).ReturnsAsync(booking);

        // Configura o mock de conflito (com o ID do booking excluído) para retornar VAZIO
        _mockBookingRepo.Setup(repo => repo.GetConflictingBookingsAsync(
            provider.Id,
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            bookingId // Importante: exclui o próprio agendamento da verificação
        )).ReturnsAsync(new List<Booking>());

        _mockMapper.Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>())).Returns(new BookingDTO { Id = bookingId });

        // Act
        var result = await _bookingService.UpdateBookingAsync(bookingId, userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.InitialDate, booking.InitialDate); // Verifica se a data foi atualizada
        Assert.Equal(BookingStatus.Pending, booking.Status); // Verifica se o status foi resetado
        _mockBookingRepo.Verify(repo => repo.Update(booking), Times.Once);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookingAsync_ShouldReturnNull_WhenBookingNotFound()
    {
        // Arrange
        _mockBookingRepo.Setup(repo => repo.GetByIdAndUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((Booking?)null);

        // Act
        var result = await _bookingService.UpdateBookingAsync(99, 1, new BookingForRescheduleDTO());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateBookingAsync_ShouldThrowInvalidOperation_WhenNewSlotIsFull()
    {
        // Arrange
        var bookingId = 1;
        var userId = 1;
        var dto = new BookingForRescheduleDTO { InitialDate = DateTime.UtcNow.AddDays(1) };

        var service = new ServiceOffering { Id = 1, TotalHours = 1 };
        var provider = new Provider { Id = 1, ConcurrentCapacity = 1 };
        var user = new User { Id = 1 };
        var booking = new Booking(service, provider, user) { Id = bookingId, UserId = userId, ProviderId = 1 };

        _mockBookingRepo.Setup(repo => repo.GetByIdAndUserIdAsync(bookingId, userId)).ReturnsAsync(booking);

        // Simula um conflito (outro agendamento já existe no novo horário)
        _mockBookingRepo.Setup(repo => repo.GetConflictingBookingsAsync(
            provider.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), bookingId
        )).ReturnsAsync(new List<Booking> { new Booking(service, provider, user) }); // Retorna 1 conflito

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookingService.UpdateBookingAsync(bookingId, userId, dto)
        );
        Assert.Equal("Novo Provedor sem horário disponível", exception.Message);
    }

    #endregion

    #region ConfirmBookingAsync Tests

    [Fact]
    public async Task ConfirmBookingAsync_ShouldConfirmBooking_WhenCalledByCorrectProvider()
    {
        // Arrange
        var bookingId = 1;
        var providerUserId = 10;
        var providerId = 5;

        var provider = new Provider { Id = providerId, UserId = providerUserId };
        var service = new ServiceOffering { Id = 1, TotalHours = 1 };
        var user = new User { Id = 1 };
        var booking = new Booking(service, provider, user) { Id = bookingId, ProviderId = providerId, Status = BookingStatus.Pending };

        _mockBookingRepo.Setup(repo => repo.GetByIdWithDetailsAsync(bookingId)).ReturnsAsync(booking);
        _mockProviderRepo.Setup(repo => repo.GetByUserIdAsync(providerUserId)).ReturnsAsync(provider);
        _mockMapper.Setup(m => m.Map<BookingDTO>(booking)).Returns(new BookingDTO { Id = bookingId, Status = "Confirmed" });

        // Act
        var result = await _bookingService.ConfirmBookingAsync(bookingId, providerUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        _mockBookingRepo.Verify(repo => repo.Update(booking), Times.Once);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmBookingAsync_ShouldThrowUnauthorized_WhenProviderIsInvalid()
    {
        // Arrange
        _mockBookingRepo.Setup(repo => repo.GetByIdWithDetailsAsync(1)).ReturnsAsync(new Booking(new ServiceOffering(), new Provider(), new User()));
        _mockProviderRepo.Setup(repo => repo.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync((Provider?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _bookingService.ConfirmBookingAsync(1, 99)
        );
        Assert.Equal("O usuário atual não é um provedor válido.", exception.Message);
    }

    [Fact]
    public async Task ConfirmBookingAsync_ShouldThrowUnauthorized_WhenProviderDoesNotOwnBooking()
    {
        // Arrange
        var provider = new Provider { Id = 5, UserId = 10 }; // Provider 5
        var otherProvider = new Provider { Id = 99 };
        var booking = new Booking(new ServiceOffering(), otherProvider, new User()) { Id = 1, ProviderId = 99, Status = BookingStatus.Pending }; // Booking do Provider 99

        _mockBookingRepo.Setup(repo => repo.GetByIdWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockProviderRepo.Setup(repo => repo.GetByUserIdAsync(10)).ReturnsAsync(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _bookingService.ConfirmBookingAsync(1, 10)
        );
        Assert.Equal("Este provedor não tem permissão para confirmar este agendamento.", exception.Message);
    }

    [Fact]
    public async Task ConfirmBookingAsync_ShouldThrowInvalidOperation_WhenBookingIsNotPending()
    {
        // Arrange
        var providerId = 5;
        var providerUserId = 10;
        var provider = new Provider { Id = providerId, UserId = providerUserId };
        var booking = new Booking(new ServiceOffering(), provider, new User())
        {
            Id = 1,
            ProviderId = providerId,
            Status = BookingStatus.Cancelled // Status NÃO PENDENTE
        };

        _mockBookingRepo.Setup(repo => repo.GetByIdWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockProviderRepo.Setup(repo => repo.GetByUserIdAsync(10)).ReturnsAsync(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookingService.ConfirmBookingAsync(1, 10)
        );
        Assert.Equal("Este agendamento não pode mais ser confirmado.", exception.Message);
    }

    #endregion

    // Observação: Os testes para GetBookingAsync, GetBookingsByUserIdAsync e GetBookingsByProvidersAsync
    // são testes de "leitura" (GET) mais simples, que envolvem principalmente
    // verificar se o repositório é chamado e se o mapper é chamado.
    // Os testes acima focam nas lógicas de negócio mais críticas (Create, Update, Cancel, Confirm).
}