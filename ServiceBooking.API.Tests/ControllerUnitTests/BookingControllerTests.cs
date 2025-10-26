using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceBooking.API.Controllers;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Shared.Common;
using System.Security.Claims;

namespace ServiceBooking.API.Tests.ControllerUnitTests;

// Documentação: Esta classe testa o BookingController
public class BookingControllerTests
{
    // Mocks para as dependências
    private readonly Mock<IBookingService> _mockBookingService;
    private readonly Mock<ITokenService> _mockTokenService; // Embora não usado nos métodos, está no construtor

    // Instância do controlador
    private readonly BookingController _bookingController;

    public BookingControllerTests()
    {
        _mockBookingService = new Mock<IBookingService>();
        _mockTokenService = new Mock<ITokenService>();

        _bookingController = new BookingController(_mockBookingService.Object, _mockTokenService.Object);

        // ESSENCIAL: Controladores dependem de um HttpContext.
        // Precisamos simular um HttpContext com um utilizador (ClaimsPrincipal)
        // para que os [Authorize] e as chamadas a 'User.FindFirst' funcionem.
        _bookingController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // Cria um contexto HTTP simulado
        };
    }

    /// <summary>
    /// Método auxiliar para simular um utilizador logado (definindo os Claims).
    /// </summary>
    private void SetupUserClaims(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Atribui o utilizador simulado ao contexto do controlador
        _bookingController.ControllerContext.HttpContext.User = claimsPrincipal;
    }

    #region RegisterBooking Tests

    [Fact]
    public async Task RegisterBooking_ShouldReturnCreatedAtAction_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = "1";
        SetupUserClaims(userId, "User"); // Simula um cliente logado

        var dto = new BookingForRegistrationDTO { ProviderId = 1, ServiceOfferingId = 1 };
        var createdBookingDto = new BookingDTO { Id = 10, UserId = int.Parse(userId) };

        _mockBookingService.Setup(s => s.CreateBookingAsync(dto, int.Parse(userId)))
                           .ReturnsAsync(createdBookingDto);

        // Act
        var result = await _bookingController.RegisterBooking(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(BookingController.GetBookingById), createdResult.ActionName);
        Assert.Equal(createdBookingDto.Id, createdResult.RouteValues["id"]);
    }

    #endregion

    #region GetMyBookings Tests

    [Fact]
    public async Task GetMyBookings_ShouldReturnOkWithPagedResult_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = "1";
        SetupUserClaims(userId, "User"); // Simula um cliente logado

        var queryParams = new QueryParameters();
        var bookings = new List<BookingDTO> { new BookingDTO { Id = 1, UserId = 1 } };
        var pagedResult = new PagedResult<BookingDTO>(bookings, 1, 1, 10);

        _mockBookingService.Setup(s => s.GetBookingsByUserIdAsync(int.Parse(userId), queryParams))
                           .ReturnsAsync(pagedResult);

        // Act
        var result = await _bookingController.GetMyBookings(queryParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var returnedPagedResult = Assert.IsAssignableFrom<IEnumerable<BookingDTO>>(okResult.Value);
        Assert.Single(returnedPagedResult); // Verifica se os itens da página foram retornados
    }

    [Fact]
    public async Task GetMyBookings_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        // Arrange
        // Simula um utilizador SEM o Claim de ID (NameIdentifier)
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, "User") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _bookingController.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        // Act
        var result = await _bookingController.GetMyBookings(new QueryParameters());

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
        Assert.Equal("Token inválido ou não contém o ID do usuário.", unauthorizedResult.Value);
    }

    #endregion

    #region CancelBooking Tests

    [Fact]
    public async Task CancelBooking_ShouldReturnNoContent_WhenUserCancelsOwnBooking()
    {
        // Arrange
        var userId = "1";
        var bookingId = 10;
        SetupUserClaims(userId, "User"); // Simula um cliente logado

        // Simula que o cancelamento foi bem-sucedido
        _mockBookingService.Setup(s => s.CancelAsync(bookingId, int.Parse(userId), false)) // itsProvider = false
                           .ReturnsAsync(true);

        // Act
        var result = await _bookingController.CancelBooking(bookingId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task CancelBooking_ShouldReturnNoContent_WhenProviderCancelsBooking()
    {
        // Arrange
        var providerUserId = "2";
        var bookingId = 10;
        SetupUserClaims(providerUserId, "Provider"); // Simula um provedor logado

        // Simula que o cancelamento foi bem-sucedido
        _mockBookingService.Setup(s => s.CancelAsync(bookingId, int.Parse(providerUserId), true)) // itsProvider = true
                           .ReturnsAsync(true);

        // Act
        var result = await _bookingController.CancelBooking(bookingId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task CancelBooking_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var userId = "1";
        var bookingId = 99; // ID inexistente
        SetupUserClaims(userId, "User");

        // Simula que o serviço não encontrou o agendamento (retorna false)
        _mockBookingService.Setup(s => s.CancelAsync(bookingId, int.Parse(userId), false))
                           .ReturnsAsync(false);

        // Act
        var result = await _bookingController.CancelBooking(bookingId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("Agendamento não encontrado ou não pertence a este usuário.", notFoundResult.Value);
    }

    #endregion

    #region ConfirmBooking Tests

    [Fact]
    public async Task ConfirmBooking_ShouldReturnOk_WhenProviderConfirms()
    {
        // Arrange
        var providerUserId = "2";
        var bookingId = 10;
        SetupUserClaims(providerUserId, "Provider"); // Simula um provedor logado

        // Agora esperamos um BookingDTO (graças à correção no controlador)
        var confirmedBookingDto = new BookingDTO { Id = bookingId, Status = "Confirmed" };

        _mockBookingService.Setup(s => s.ConfirmBookingAsync(bookingId, int.Parse(providerUserId)))
                           .ReturnsAsync(confirmedBookingDto);

        // Act
        // O tipo de 'actionResult' é ActionResult<BookingDTO>
        var actionResult = await _bookingController.ConfirmBooking(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var returnValue = Assert.IsType<BookingDTO>(okResult.Value);
        Assert.Equal(confirmedBookingDto, returnValue);
    }

    [Fact]
    public async Task ConfirmBooking_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var providerUserId = "2";
        var bookingId = 99;
        SetupUserClaims(providerUserId, "Provider");

        _mockBookingService.Setup(s => s.ConfirmBookingAsync(bookingId, int.Parse(providerUserId)))
                           .ReturnsAsync((BookingDTO?)null); // Serviço não encontra o agendamento

        // Act
        var actionResult = await _bookingController.ConfirmBooking(bookingId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("ID do agendamento não encontrado", notFoundResult.Value);
    }

    #endregion
}