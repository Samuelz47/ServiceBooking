using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceBooking.API.Controllers;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Shared.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Para o esquema "Bearer"

namespace ServiceBooking.API.Tests.ControllerUnitTests;

// Documentação: Esta classe testa o ProviderController
public class ProviderControllerTests
{
    // Mock da dependência
    private readonly Mock<IProviderService> _mockProviderService;

    // Instância do controlador
    private readonly ProviderController _providerController;

    public ProviderControllerTests()
    {
        _mockProviderService = new Mock<IProviderService>();

        _providerController = new ProviderController(_mockProviderService.Object);

        // Configura um HttpContext simulado para os testes de autorização
        _providerController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
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
        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme); // Usa o esquema "Bearer"
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _providerController.ControllerContext.HttpContext.User = claimsPrincipal;
    }

    #region GetProviderById Tests

    [Fact]
    public async Task GetProviderById_ShouldReturnOk_WhenProviderExists()
    {
        // Arrange
        var providerId = 1;
        var providerDto = new ProviderDetailsDto { Id = providerId, Name = "Provider Teste" };

        _mockProviderService.Setup(s => s.GetAsync(providerId))
                            .ReturnsAsync(providerDto);

        // Act
        var result = await _providerController.GetProviderById(providerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(providerDto, okResult.Value);
    }

    [Fact]
    public async Task GetProviderById_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Arrange
        _mockProviderService.Setup(s => s.GetAsync(It.IsAny<int>()))
                            .ReturnsAsync((ProviderDetailsDto?)null);

        // Act
        var result = await _providerController.GetProviderById(99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("Nenhum provedor encontrado", notFoundResult.Value);
    }

    #endregion

    #region GetAllProvidersAsync Tests

    [Fact]
    public async Task GetAllProvidersAsync_ShouldReturnOkWithPagedResult()
    {
        // Arrange
        var queryParams = new QueryParameters();
        var providers = new List<ProviderDto> { new ProviderDto { Id = 1, Name = "Provider 1" } };
        var pagedResult = new PagedResult<ProviderDto>(providers, 1, 1, 10);

        _mockProviderService.Setup(s => s.GetAllAsync(queryParams))
                            .ReturnsAsync(pagedResult);

        // Act
        var result = await _providerController.GetAllProvidersAsync(queryParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<ProviderDto>>(okResult.Value);
        Assert.Single(returnedItems);
    }

    #endregion

    #region RegisterProviderAsync Tests

    [Fact]
    public async Task RegisterProviderAsync_ShouldReturnCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var dto = new ProviderForRegistrationDto { Name = "Novo Provider" };
        var createdDto = new ProviderDto { Id = 5, Name = "Novo Provider" };

        _mockProviderService.Setup(s => s.CreateProviderWithUserAsync(dto))
                            .ReturnsAsync(createdDto);

        // Act
        var actionResult = await _providerController.RegisterProviderAsync(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(ProviderController.GetProviderById), createdResult.ActionName);
        Assert.Equal(createdDto.Id, createdResult.RouteValues["id"]);
    }

    #endregion

    #region UpdateProviderAsync Tests

    [Fact]
    public async Task UpdateProviderAsync_ShouldReturnOk_WhenProviderExists()
    {
        // Arrange
        SetupUserClaims("1", "Provider"); // Endpoint requer autenticação
        var providerId = 1;
        var dto = new ProviderForUpdateDTO { Name = "Nome Atualizado" };
        var updatedDto = new ProviderDto { Id = providerId, Name = "Nome Atualizado" };

        _mockProviderService.Setup(s => s.UpdateAsync(dto, providerId))
                            .ReturnsAsync(updatedDto);

        // Act
        var actionResult = await _providerController.UpdateProviderAsync(dto, providerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedDto = Assert.IsType<ProviderDto>(okResult.Value);
        Assert.Equal(updatedDto.Name, returnedDto.Name);
    }

    [Fact]
    public async Task UpdateProviderAsync_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Testando com Role Admin também
        var providerId = 99;
        var dto = new ProviderForUpdateDTO();

        _mockProviderService.Setup(s => s.UpdateAsync(dto, providerId))
                            .ReturnsAsync((ProviderDto?)null);

        // Act
        var actionResult = await _providerController.UpdateProviderAsync(dto, providerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region UpdateServicesOfProvider Tests

    [Fact]
    public async Task UpdateServicesOfProvider_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        SetupUserClaims("1", "Provider"); // Endpoint requer autenticação
        var providerId = 1;
        var dto = new ProviderUpdateServicesDTO { ServicesIds = new List<int> { 1, 2 } };
        var updatedDto = new ProviderDetailsDto { Id = providerId, Name = "Provider com Serviços" };

        _mockProviderService.Setup(s => s.UpdateServicesAsync(dto, providerId))
                            .ReturnsAsync(updatedDto);

        // Act
        var actionResult = await _providerController.UpdateServicesOfProvider(dto, providerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.IsType<ProviderDetailsDto>(okResult.Value);
    }

    [Fact]
    public async Task UpdateServicesOfProvider_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Arrange
        SetupUserClaims("1", "Provider"); // Endpoint requer autenticação
        var providerId = 99;
        var dto = new ProviderUpdateServicesDTO();

        _mockProviderService.Setup(s => s.UpdateServicesAsync(dto, providerId))
                            .ReturnsAsync((ProviderDetailsDto?)null);

        // Act
        var actionResult = await _providerController.UpdateServicesOfProvider(dto, providerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region DeleteProviderAsync Tests

    [Fact]
    public async Task DeleteProviderAsync_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Endpoint requer Admin
        var providerId = 1;

        _mockProviderService.Setup(s => s.DeleteAsync(providerId))
                            .ReturnsAsync(true); // Simula sucesso

        // Act
        var result = await _providerController.DeleteProviderAsync(providerId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task DeleteProviderAsync_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Endpoint requer Admin
        var providerId = 99;

        _mockProviderService.Setup(s => s.DeleteAsync(providerId))
                            .ReturnsAsync(false); // Simula falha (não encontrado)

        // Act
        var result = await _providerController.DeleteProviderAsync(providerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion
}