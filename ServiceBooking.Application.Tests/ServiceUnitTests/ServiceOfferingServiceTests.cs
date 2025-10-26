using AutoMapper;
using Moq;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Services;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Shared.Common;
using System.Linq.Expressions;

namespace ServiceBooking.Application.Tests.ServiceUnitTests;

// Documentação: Esta classe testa o ServiceOfferingService
public class ServiceOfferingServiceTests
{
    // Mocks para as dependências do ServiceOfferingService
    private readonly Mock<IServiceOfferingRepository> _mockServiceRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUnitOfWork> _mockUof;

    // A instância real do serviço que estamos testando
    private readonly ServiceOfferingService _serviceOfferingService;

    public ServiceOfferingServiceTests()
    {
        // 1. Inicializa todos os mocks
        _mockServiceRepo = new Mock<IServiceOfferingRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockUof = new Mock<IUnitOfWork>();

        // 2. Cria a instância do serviço, injetando os mocks
        _serviceOfferingService = new ServiceOfferingService(
            _mockServiceRepo.Object,
            _mockMapper.Object,
            _mockUof.Object
        );
    }

    #region RegisterServiceAsync Tests

    [Fact]
    public async Task RegisterServiceAsync_ShouldCreateService_WhenNameIsNew()
    {
        // Arrange (Preparar)
        var dto = new ServiceOfferingForRegistrationDTO { Name = "Corte de Cabelo", TotalHours = 1 };
        var serviceEntity = new ServiceOffering { Id = 1, Name = "Corte de Cabelo", TotalHours = 1 };
        var expectedDto = new ServiceOfferingDTO { Id = 1, Name = "Corte de Cabelo", TotalHours = 1 };

        // 1. Simula que o serviço NÃO existe (GetAsync retorna nulo)
        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync((ServiceOffering?)null);

        // 2. Configura o AutoMapper para "traduzir" o DTO para a Entidade
        _mockMapper.Setup(m => m.Map<ServiceOffering>(dto)).Returns(serviceEntity);

        // 3. Configura o AutoMapper para "traduzir" a Entidade de volta para o DTO de resposta
        _mockMapper.Setup(m => m.Map<ServiceOfferingDTO>(serviceEntity)).Returns(expectedDto);

        // 4. Simula o Commit (salvar no banco)
        _mockUof.Setup(uof => uof.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act (Agir)
        var result = await _serviceOfferingService.RegisterServiceAsync(dto);

        // Assert (Verificar)
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);
        _mockServiceRepo.Verify(repo => repo.Create(serviceEntity), Times.Once); // Verifica se o Create foi chamado
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once); // Verifica se o Commit foi chamado
    }

    [Fact]
    public async Task RegisterServiceAsync_ShouldThrowInvalidOperationException_WhenNameExists()
    {
        // Arrange
        var dto = new ServiceOfferingForRegistrationDTO { Name = "Serviço Existente" };
        var existingService = new ServiceOffering { Id = 1, Name = "Serviço Existente" };

        // 1. Simula que o serviço JÁ EXISTE (GetAsync retorna uma entidade)
        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync(existingService);

        // Act & Assert
        // Verifica se a exceção correta é lançada
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _serviceOfferingService.RegisterServiceAsync(dto)
        );

        // Verifica se a mensagem da exceção é a que definimos no serviço
        Assert.Equal("Esse serviço já foi cadastrado", exception.Message);

        // Garante que nada foi criado ou salvo
        _mockServiceRepo.Verify(repo => repo.Create(It.IsAny<ServiceOffering>()), Times.Never);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateServiceOfferingAsync Tests

    [Fact]
    public async Task UpdateServiceOfferingAsync_ShouldUpdateService_WhenServiceExists()
    {
        // Arrange
        var serviceId = 1;
        var dto = new ServiceOfferingForUpdateDTO { Name = "Nome Atualizado", Description = "Desc Atualizada", TotalHours = 2 };

        // Simula a entidade original que veio do banco
        var originalService = new ServiceOffering { Id = serviceId, Name = "Nome Antigo", Description = "Desc Antiga", TotalHours = 1 };

        // Simula o DTO de resposta que o Mapper vai retornar
        var expectedDto = new ServiceOfferingDTO { Id = serviceId, Name = "Nome Atualizado", Description = "Desc Atualizada", TotalHours = 2 };

        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync(originalService);
        _mockUof.Setup(uof => uof.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<ServiceOfferingDTO>(originalService)).Returns(expectedDto);

        // Act
        var result = await _serviceOfferingService.UpdateServiceOfferingAsync(dto, serviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);

        // Verifica se a entidade *original* foi modificada ANTES do commit
        Assert.Equal("Nome Atualizado", originalService.Name);
        Assert.Equal("Desc Atualizada", originalService.Description);
        Assert.Equal(2, originalService.TotalHours);

        _mockServiceRepo.Verify(repo => repo.Update(originalService), Times.Once);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateServiceOfferingAsync_ShouldReturnNull_WhenServiceNotFound()
    {
        // Arrange
        var dto = new ServiceOfferingForUpdateDTO();
        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync((ServiceOffering?)null);

        // Act
        var result = await _serviceOfferingService.UpdateServiceOfferingAsync(dto, 99); // 99 = ID inexistente

        // Assert
        Assert.Null(result);
        _mockServiceRepo.Verify(repo => repo.Update(It.IsAny<ServiceOffering>()), Times.Never);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenServiceExists()
    {
        // Arrange
        var serviceId = 1;
        var serviceEntity = new ServiceOffering { Id = serviceId, Name = "Serviço Para Deletar" };

        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync(serviceEntity);
        _mockUof.Setup(uof => uof.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _serviceOfferingService.DeleteAsync(serviceId);

        // Assert
        Assert.True(result);
        _mockServiceRepo.Verify(repo => repo.Delete(serviceEntity), Times.Once); // Verifica se o Delete foi chamado com a entidade correta
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenServiceNotFound()
    {
        // Arrange
        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync((ServiceOffering?)null);

        // Act
        var result = await _serviceOfferingService.DeleteAsync(99);

        // Assert
        Assert.False(result);
        _mockServiceRepo.Verify(repo => repo.Delete(It.IsAny<ServiceOffering>()), Times.Never);
        _mockUof.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetServiceAsync Tests

    [Fact]
    public async Task GetServiceAsync_ShouldReturnDto_WhenServiceExists()
    {
        // Arrange
        var serviceId = 1;
        var serviceEntity = new ServiceOffering { Id = serviceId, Name = "Serviço Teste" };
        var expectedDto = new ServiceOfferingDetailsDTO { Id = serviceId, Name = "Serviço Teste" };

        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync(serviceEntity);
        _mockMapper.Setup(m => m.Map<ServiceOfferingDetailsDTO>(serviceEntity)).Returns(expectedDto);

        // Act
        var result = await _serviceOfferingService.GetServiceAsync(serviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
    }

    [Fact]
    public async Task GetServiceAsync_ShouldReturnNull_WhenServiceNotFound()
    {
        // Arrange
        _mockServiceRepo.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ServiceOffering, bool>>>()))
                        .ReturnsAsync((ServiceOffering?)null);

        // Act
        var result = await _serviceOfferingService.GetServiceAsync(99);

        // Assert
        Assert.Null(result);
        _mockMapper.Verify(m => m.Map<ServiceOfferingDetailsDTO>(It.IsAny<ServiceOffering>()), Times.Never);
    }

    #endregion

    #region GetAllServicesAsync Tests

    [Fact]
    public async Task GetAllServicesAsync_ShouldReturnPagedResultOfDtos()
    {
        // Arrange
        var queryParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
        var serviceEntities = new List<ServiceOffering>
        {
            new ServiceOffering { Id = 1, Name = "Serviço 1" },
            new ServiceOffering { Id = 2, Name = "Serviço 2" }
        };
        var totalCount = 2;

        // O repositório retorna um PagedResult de ENTIDADES
        var pagedResultFromRepo = new PagedResult<ServiceOffering>(serviceEntities, queryParams.PageNumber, queryParams.PageSize, totalCount);

        var dtos = new List<ServiceOfferingDTO>
        {
            new ServiceOfferingDTO { Id = 1, Name = "Serviço 1" },
            new ServiceOfferingDTO { Id = 2, Name = "Serviço 2" }
        };

        _mockServiceRepo.Setup(repo => repo.GetAllAsync(queryParams)).ReturnsAsync(pagedResultFromRepo);
        _mockMapper.Setup(m => m.Map<IEnumerable<ServiceOfferingDTO>>(serviceEntities)).Returns(dtos);

        // Act
        var result = await _serviceOfferingService.GetAllServicesAsync(queryParams);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(queryParams.PageNumber, result.PageNumber);
        Assert.Equal(dtos.Count, result.Items.Count());
        Assert.Equal(dtos.First().Name, result.Items.First().Name);
    }

    #endregion
}
