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

// Documentação: Esta classe testa o ServiceOfferingController
public class ServiceOfferingControllerTests
{
    // Mock da dependência
    private readonly Mock<IServiceOfferingService> _mockServiceOfferingService;

    // Instância do controlador
    private readonly ServiceOfferingController _serviceOfferingController;

    public ServiceOfferingControllerTests()
    {
        _mockServiceOfferingService = new Mock<IServiceOfferingService>();

        _serviceOfferingController = new ServiceOfferingController(_mockServiceOfferingService.Object);

        // Configura um HttpContext simulado para os testes de autorização
        _serviceOfferingController.ControllerContext = new ControllerContext
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

        _serviceOfferingController.ControllerContext.HttpContext.User = claimsPrincipal;
    }

    #region GetServiceOfferingById Tests

    [Fact]
    public async Task GetServiceOfferingById_ShouldReturnOk_WhenServiceExists()
    {
        // Arrange
        var serviceId = 1;
        var serviceDto = new ServiceOfferingDetailsDTO { Id = serviceId, Name = "Serviço Teste" };

        _mockServiceOfferingService.Setup(s => s.GetServiceAsync(serviceId))
                                   .ReturnsAsync(serviceDto);

        // Act
        var result = await _serviceOfferingController.GetServiceOfferingById(serviceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(serviceDto, okResult.Value);
    }

    [Fact]
    public async Task GetServiceOfferingById_ShouldReturnNotFound_WhenServiceDoesNotExist()
    {
        // Arrange
        _mockServiceOfferingService.Setup(s => s.GetServiceAsync(It.IsAny<int>()))
                                   .ReturnsAsync((ServiceOfferingDetailsDTO?)null);

        // Act
        var result = await _serviceOfferingController.GetServiceOfferingById(99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("Nenhum servidor encontrado", notFoundResult.Value);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnOkWithPagedResult()
    {
        // Arrange
        var queryParams = new QueryParameters();
        var services = new List<ServiceOfferingDTO> { new ServiceOfferingDTO { Id = 1 } };
        var pagedResult = new PagedResult<ServiceOfferingDTO>(services, 1, 1, 10);

        _mockServiceOfferingService.Setup(s => s.GetAllServicesAsync(queryParams))
                                   .ReturnsAsync(pagedResult);

        // Act
        var result = await _serviceOfferingController.GetAllAsync(queryParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<ServiceOfferingDTO>>(okResult.Value);
        Assert.Single(returnedItems);
    }

    #endregion

    #region RegisterServiceAsync Tests

    [Fact]
    public async Task RegisterServiceAsync_ShouldReturnCreatedAtAction_WhenAdminIsAuthenticated()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Simula um Admin logado
        var dto = new ServiceOfferingForRegistrationDTO { Name = "Novo Serviço" };
        var createdDto = new ServiceOfferingDTO { Id = 5, Name = "Novo Serviço" };

        _mockServiceOfferingService.Setup(s => s.RegisterServiceAsync(dto))
                                   .ReturnsAsync(createdDto);

        // Act
        // O tipo de 'actionResult' é ActionResult<ServiceOfferingForRegistrationDTO>, mas o retorno é <ServiceOfferingDTO>
        var actionResult = await _serviceOfferingController.RegisterServiceAsync(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result); // Verificamos a propriedade .Result
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(ServiceOfferingController.GetServiceOfferingById), createdResult.ActionName);
        Assert.Equal(createdDto.Id, createdResult.RouteValues["id"]);
    }

    [Fact]
    public async Task RegisterServiceAsync_ShouldReturnCreatedAtAction_WhenProviderIsAuthenticated()
    {
        // Arrange
        SetupUserClaims("2", "Provider"); // Simula um Provider logado
        var dto = new ServiceOfferingForRegistrationDTO { Name = "Novo Serviço" };
        var createdDto = new ServiceOfferingDTO { Id = 5, Name = "Novo Serviço" };

        _mockServiceOfferingService.Setup(s => s.RegisterServiceAsync(dto))
                                   .ReturnsAsync(createdDto);

        // Act
        var actionResult = await _serviceOfferingController.RegisterServiceAsync(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result); // Verificamos a propriedade .Result
        Assert.Equal(201, createdResult.StatusCode);
    }

    #endregion

    #region UpdateServiceAsync Tests

    [Fact]
    public async Task UpdateServiceAsync_ShouldReturnOk_WhenAdminIsAuthenticatedAndServiceExists()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Simula um Admin logado
        var serviceId = 1;
        var dto = new ServiceOfferingForUpdateDTO { Name = "Nome Atualizado" };
        var updatedDto = new ServiceOfferingDTO { Id = serviceId, Name = "Nome Atualizado" };

        _mockServiceOfferingService.Setup(s => s.UpdateServiceOfferingAsync(dto, serviceId))
                                   .ReturnsAsync(updatedDto);

        // Act
        var actionResult = await _serviceOfferingController.UpdateServiceAsync(dto, serviceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedDto = Assert.IsType<ServiceOfferingDTO>(okResult.Value);
        Assert.Equal(updatedDto.Name, returnedDto.Name);
    }

    [Fact]
    public async Task UpdateServiceAsync_ShouldReturnNotFound_WhenServiceDoesNotExist()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Autenticação é necessária
        var serviceId = 99;
        var dto = new ServiceOfferingForUpdateDTO();

        _mockServiceOfferingService.Setup(s => s.UpdateServiceOfferingAsync(dto, serviceId))
                                   .ReturnsAsync((ServiceOfferingDTO?)null);

        // Act
        var actionResult = await _serviceOfferingController.UpdateServiceAsync(dto, serviceId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal($"Nenhum serviço foi encontrado com o ID {serviceId}", notFoundResult.Value);
    }

    #endregion

    #region DeleteServiceOfferingAsync Tests

    [Fact]
    public async Task DeleteServiceOfferingAsync_ShouldReturnNoContent_WhenAdminIsAuthenticatedAndServiceExists()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Simula um Admin logado
        var serviceId = 1;

        // Simula que o serviço de delete foi bem-sucedido
        _mockServiceOfferingService.Setup(s => s.DeleteAsync(serviceId))
                                   .ReturnsAsync(true);

        // Act
        var result = await _serviceOfferingController.DeleteServiceOfferingAsync(serviceId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task DeleteServiceOfferingAsync_ShouldReturnNotFound_WhenServiceDoesNotExist()
    {
        // Arrange
        SetupUserClaims("1", "Admin"); // Autenticação é necessária
        var serviceId = 99;

        // Simula que o serviço de delete falhou (não encontrou)
        _mockServiceOfferingService.Setup(s => s.DeleteAsync(serviceId))
                                   .ReturnsAsync(false);

        // Act
        var result = await _serviceOfferingController.DeleteServiceOfferingAsync(serviceId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal($"Nenhum serviço foi encontrado com o ID {serviceId}", notFoundResult.Value);
    }

    #endregion
}