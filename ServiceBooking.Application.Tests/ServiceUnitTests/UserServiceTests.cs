using AutoMapper;
using Moq;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Services;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using System.Linq.Expressions;
using ServiceBooking.Application.Interfaces; // Necessário para ITokenService

namespace ServiceBooking.Application.Tests.ServiceUnitTests;

// Documentação: Esta classe testa o UserService
public class UserServiceTests
{
    // Mocks para as dependências do UserService
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IUnitOfWork> _mockUof;
    private readonly Mock<IMapper> _mockMapper;

    // A instância real do serviço que estamos testando
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // 1. Inicializa todos os mocks
        _mockUserRepo = new Mock<IUserRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockUof = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        // 2. Cria a instância do serviço, injetando os mocks
        _userService = new UserService(
            _mockUserRepo.Object,
            _mockTokenService.Object,
            _mockUof.Object,
            _mockMapper.Object
        );
    }

    #region RegisterUserAsync Tests

    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUser_WhenEmailIsNew()
    {
        // Arrange (Preparar)
        var dto = new UserForRegistrationDto { Name = "Novo Usuário", Email = "novo@email.com", Password = "Password123" };
        var userEntity = new User { Id = 1, Name = "Novo Usuário", Email = "novo@email.com" };
        var expectedDto = new UserDTO { Id = 1, Name = "Novo Usuário", Email = "novo@email.com", Role = "Client" };

        // 1. Simula que o e-mail NÃO existe
        _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

        // 2. Configura os Mappers
        _mockMapper.Setup(m => m.Map<User>(dto)).Returns(userEntity);
        _mockMapper.Setup(m => m.Map<UserDTO>(userEntity)).Returns(expectedDto);

        // 3. Simula o Commit
        _mockUof.Setup(uof => uof.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act (Agir)
        var result = await _userService.RegisterUserAsync(dto);

        // Assert (Verificar)
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Email, result.Email);
        Assert.Equal("Client", result.Role); // Verifica se a Role foi definida corretamente

        // Verifica se os métodos corretos foram chamados
        _mockUserRepo.Verify(repo => repo.GetUserByEmailAsync(dto.Email), Times.Once);
        _mockUserRepo.Verify(repo => repo.AddUserAsync(It.Is<User>(u => u.Email == dto.Email)), Times.Once);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verifica se a senha foi hasheada (não podemos saber o valor exato, mas podemos saber que não é a senha original)
        Assert.NotEqual(dto.Password, userEntity.Password);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowInvalidOperationException_WhenEmailExists()
    {
        // Arrange
        var dto = new UserForRegistrationDto { Email = "existente@email.com" };
        var existingUser = new User { Id = 1, Email = "existente@email.com" };

        // 1. Simula que o e-mail JÁ EXISTE
        _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.RegisterUserAsync(dto)
        );

        Assert.Equal("Este email já foi cadastrado", exception.Message);
        _mockUserRepo.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Never);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var password = "SenhaForte123";
        // Precisamos usar o BCrypt real para criar um hash, 
        // pois o serviço usa o BCrypt real para verificar.
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var userEmail = "teste@email.com";
        var user = new User { Id = 1, Email = userEmail, Name = "Usuário Teste", Password = hashedPassword, Role = "Client" };
        var loginDto = new LoginDTO { Email = userEmail, Password = password }; // Senha correta

        var expectedToken = "ey.JhbGciOiJIUzI1NiJ9.dummy_token_string";

        // 1. Simula o repositório encontrando o usuário
        _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(userEmail)).ReturnsAsync(user);

        // 2. Simula o serviço de token gerando um token
        _mockTokenService.Setup(s => s.GenerateToken(user)).Returns(expectedToken);

        // Act
        var resultToken = await _userService.Login(loginDto);

        // Assert
        Assert.NotNull(resultToken);
        Assert.Equal(expectedToken, resultToken);
        _mockTokenService.Verify(s => s.GenerateToken(user), Times.Once); // Verifica se o token foi gerado
    }

    [Fact]
    public async Task Login_ShouldThrowInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var loginDto = new LoginDTO { Email = "naoexiste@email.com", Password = "123" };

        // 1. Simula o repositório NÃO encontrando o usuário
        _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.Login(loginDto)
        );

        Assert.Equal("Email ou senha inválidos", exception.Message);
        _mockTokenService.Verify(s => s.GenerateToken(It.IsAny<User>()), Times.Never); // Token NUNCA deve ser gerado
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        // Arrange
        var password = "SenhaForte123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var userEmail = "teste@email.com";
        var user = new User { Id = 1, Email = userEmail, Password = hashedPassword };

        // O DTO envia a SENHA ERRADA
        var loginDto = new LoginDTO { Email = userEmail, Password = "SenhaERRADA" };

        // 1. Simula o repositório encontrando o usuário (com a senha "certa" hasheada)
        _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(userEmail)).ReturnsAsync(user);

        // Act & Assert
        // A lógica do BCrypt.Verify(dto.Password, user.Password) vai falhar
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.Login(loginDto)
        );

        Assert.Equal("Email ou senha inválidos", exception.Message);
        _mockTokenService.Verify(s => s.GenerateToken(It.IsAny<User>()), Times.Never); // Token NUNCA deve ser gerado
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_ShouldReturnUserDTO_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var userEntity = new User { Id = userId, Name = "Usuário Teste", Email = "teste@email.com" };
        var expectedDto = new UserDTO { Id = userId, Name = "Usuário Teste", Email = "teste@email.com" };

        _mockUserRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(userEntity);
        _mockMapper.Setup(m => m.Map<UserDTO>(userEntity)).Returns(expectedDto);

        // Act
        var result = await _userService.GetAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        _mockMapper.Verify(m => m.Map<UserDTO>(userEntity), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetAsync(99);

        // Assert
        Assert.Null(result);
        _mockMapper.Verify(m => m.Map<UserDTO>(It.IsAny<User>()), Times.Never);
    }

    #endregion
}