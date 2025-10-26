using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceBooking.API.Controllers;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Interfaces;

namespace ServiceBooking.Application.Tests.ControllerUnitTests;

// Documentação: Esta classe testa o UserController
public class UserControllerTests
{
    // Mocks para as dependências do controlador
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMapper> _mockMapper;

    // A instância real do controlador que estamos testando
    private readonly UserController _userController;

    public UserControllerTests()
    {
        // 1. Inicializa os mocks
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>(); // Embora o mapper seja injetado, ele não é usado diretamente nos métodos do UserController, mas precisamos dele para o construtor.

        // 2. Cria a instância do controlador, injetando os mocks
        _userController = new UserController(_mockUserService.Object, _mockMapper.Object);
    }

    #region GetUser Tests

    [Fact]
    public async Task GetUser_ShouldReturnOk_WhenUserExists()
    {
        // Arrange (Preparar)
        var userId = 1;
        var userDto = new UserDTO { Id = userId, Name = "Utilizador Teste", Email = "teste@email.com" };

        // Configura o mock do SERVIÇO para retornar um DTO
        _mockUserService.Setup(service => service.GetAsync(userId))
                        .ReturnsAsync(userDto);

        // Act (Agir)
        var result = await _userController.GetUser(userId);

        // Assert (Verificar)
        // 1. Verifica se o resultado é do tipo OkObjectResult (200 OK com dados)
        var okResult = Assert.IsType<OkObjectResult>(result);

        // 2. Verifica se o StatusCode é 200
        Assert.Equal(200, okResult.StatusCode);

        // 3. Verifica se o valor retornado (o DTO) é o mesmo que o serviço nos deu
        Assert.Equal(userDto, okResult.Value);
    }

    [Fact]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = 99; // ID inexistente

        // Configura o mock do SERVIÇO para retornar nulo
        _mockUserService.Setup(service => service.GetAsync(userId))
                        .ReturnsAsync((UserDTO?)null);

        // Act
        var result = await _userController.GetUser(userId);

        // Assert
        // 1. Verifica se o resultado é do tipo NotFoundObjectResult (404 Not Found)
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        // 2. Verifica se o StatusCode é 404
        Assert.Equal(404, notFoundResult.StatusCode);

        // 3. Verifica se a mensagem de erro no corpo é a esperada
        Assert.Equal("Nenhum usuário encontrado", notFoundResult.Value);
    }

    #endregion

    #region RegisterUserAsync Tests

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var registrationDto = new UserForRegistrationDto { Name = "Novo Utilizador", Email = "novo@email.com", Password = "123" };
        var createdUserDto = new UserDTO { Id = 5, Name = "Novo Utilizador", Email = "novo@email.com" }; // Simula o DTO retornado pelo serviço

        _mockUserService.Setup(service => service.RegisterUserAsync(registrationDto))
                        .ReturnsAsync(createdUserDto);

        // Act
        var result = await _userController.RegisterUserAsync(registrationDto);

        // Assert
        // 1. Verifica se o resultado é do tipo CreatedAtActionResult (201 Created)
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);

        // 2. Verifica se o StatusCode é 201
        Assert.Equal(201, createdResult.StatusCode);

        // 3. Verifica se ele está a apontar para a action "GetUser"
        Assert.Equal(nameof(UserController.GetUser), createdResult.ActionName);

        // 4. Verifica se o ID na rota é o ID do utilizador criado
        Assert.Equal(createdUserDto.Id, createdResult.RouteValues["id"]);

        // 5. Verifica se o corpo da resposta é o DTO do utilizador criado
        Assert.Equal(createdUserDto, createdResult.Value);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ShouldReturnOkWithToken_WhenSuccessful()
    {
        // Arrange
        var loginDto = new LoginDTO { Email = "teste@email.com", Password = "123" };
        var expectedToken = "token_jwt_simulado_123456";

        _mockUserService.Setup(service => service.Login(loginDto))
                        .ReturnsAsync(expectedToken);

        // Act
        var result = await _userController.Login(loginDto);

        // Assert
        // 1. Verifica se é um 200 OK
        var okResult = Assert.IsType<OkObjectResult>(result);

        // 2. Extrai o valor (que é um objeto anónimo: new { Token = token })
        var resultValue = okResult.Value;
        Assert.NotNull(resultValue);

        // 3. Verifica se o objeto anónimo tem a propriedade "Token" com o valor correto
        // (Usamos reflexão para ler a propriedade de um tipo anónimo)
        var tokenProperty = resultValue.GetType().GetProperty("Token");
        Assert.NotNull(tokenProperty); // Garante que a propriedade "Token" existe

        var tokenValue = tokenProperty.GetValue(resultValue, null);
        Assert.Equal(expectedToken, tokenValue); // Garante que o valor do token é o esperado
    }

    #endregion
}