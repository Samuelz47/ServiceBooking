using AutoMapper;
using Moq;
using ServiceBooking.Application.DTOs;
using ServiceBooking.Application.Services;
using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.Tests.ServiceUnitTests;
public class ProviderServiceTests
{
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ProviderService _providerService;
    private readonly Mock<IServiceOfferingRepository> _mockServiceOfferingRepository;

    public ProviderServiceTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockServiceOfferingRepository = new Mock<IServiceOfferingRepository>();

        _providerService = new ProviderService(
            _mockProviderRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockServiceOfferingRepository.Object,
            _mockUserRepository.Object
        );
    }
    [Fact]
    public async Task GetAsync_ReturnProviderDetailsDTO()
    {
        // Arange
        var providerId = 7;
        var providerEntity = new Provider { Id = providerId, Name = "Teste Provider", Description = "Desc", UserId = 1 };
        var expectedProviderDto = new ProviderDetailsDto { Id = providerId, Name = "Teste Provider", Description = "Desc" };

        // 2. Configurar o COMPORTAMENTO dos Mocks PARA ESTE TESTE ESPECÍFICO
        //    Dizemos ao mock do repositório como ele deve responder QUANDO GetAsync for chamado.
        _mockProviderRepository
            .Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())) // Quando GetAsync for chamado com qualquer filtro...
            .ReturnsAsync(providerEntity); // ...retorne a entidade de teste.

        //    Dizemos ao mock do mapeador como ele deve responder.
        _mockMapper
            .Setup(mapper => mapper.Map<ProviderDetailsDto>(providerEntity)) // Quando Map for chamado com a entidade...
            .Returns(expectedProviderDto); // ...retorne o DTO esperado.

        // Act (Agir)
        // Chama o método que queremos testar na instância _providerService (que já foi criada no construtor)
        var result = await _providerService.GetAsync(providerId);

        // Assert (Verificar)
        Assert.NotNull(result); // Garante que o resultado não é nulo
        Assert.Equal(expectedProviderDto.Id, result.Id); // Compara o Id
        Assert.Equal(expectedProviderDto.Name, result.Name); // Compara o Name
        Assert.Equal(expectedProviderDto.Description, result.Description); // Compara a Description

        // Verifica se os métodos dos mocks foram chamados como esperado (opcional, mas bom)
        _mockProviderRepository.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()), Times.Once); // Foi chamado 1 vez?
        _mockMapper.Verify(mapper => mapper.Map<ProviderDetailsDto>(providerEntity), Times.Once); // Foi chamado 1 vez?

    }

    [Fact]
    public async Task GetAsync_ReturnNull()
    {
        // Arange
        var providerId = 99;

        _mockProviderRepository
            .Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _providerService.GetAsync(providerId);

        // Assert
        Assert.Null(result);

        _mockProviderRepository.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<ProviderDetailsDto>(It.IsAny<Provider>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ReturnPagedResult()
    {
        // Arrange
        var queryParameters = new QueryParameters { PageNumber = 1 , PageSize = 5};
        var providerEntities = new List<Provider> // A lista de entidades da PÁGINA ATUAL
        {
            new Provider { Id = 1, Name = "Provider 1", Description = "Desc 1", UserId = 1 },
            new Provider { Id = 2, Name = "Provider 2", Description = "Desc 2", UserId = 2 }
        };
        var totalCountInDb = 25; // Simula o TOTAL de provedores no banco (para calcular TotalPages, etc.)

        // Cria o objeto PagedResult que o REPOSITÓRIO mockado vai retornar
        var pagedResultFromRepo = new PagedResult<Provider>(
            providerEntities,
            queryParameters.PageNumber,
            queryParameters.PageSize,
            totalCountInDb
        );

        // 3. Dados Falsos que o MAPPER deveria retornar
        var expectedProviderDtos = new List<ProviderDto> // A lista de DTOs correspondente
        {
            new ProviderDto { Id = 1, Name = "Provider 1", Description = "Desc 1" },
            new ProviderDto { Id = 2, Name = "Provider 2", Description = "Desc 2" }
        };

        _mockProviderRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<QueryParameters>()))
            .ReturnsAsync(pagedResultFromRepo);

        _mockMapper
            .Setup(mapper => mapper.Map<IEnumerable<ProviderDto>>(providerEntities))
            .Returns(expectedProviderDtos);

        // Act
        var result = await _providerService.GetAllAsync(queryParameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalCountInDb, result.TotalCount);
        Assert.Equal(queryParameters.PageNumber, result.PageNumber);
        Assert.Equal(queryParameters.PageSize, result.PageSize);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedProviderDtos.Count, result.Items.Count());
        Assert.Equal(expectedProviderDtos.First().Name, result.Items.First().Name);

        _mockProviderRepository.Verify(repo => repo.GetAllAsync(It.IsAny<QueryParameters>()), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<IEnumerable<ProviderDto>>(providerEntities), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnEmptyPagedResult()
    {
        // Arrange
        var queryParameters = new QueryParameters { PageNumber = 1, PageSize = 5 };
        var emptyProviderList = new List<Provider>();
        var emptyPagedResultFromRepo = new PagedResult<Provider>(emptyProviderList, 1, 5, 0);

        _mockProviderRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<QueryParameters>()))
            .ReturnsAsync(emptyPagedResultFromRepo);

        var emptyProviderDtoList = new List<ProviderDto>();

        _mockMapper
            .Setup(mapper => mapper.Map<IEnumerable<ProviderDto>>(emptyProviderList))
            .Returns(emptyProviderDtoList);

        // Act
        var result = await _providerService.GetAllAsync(queryParameters);

        // Assert
        Assert.NotNull(result); // O PagedResult em si não deve ser nulo
        Assert.Empty(result.Items); // A lista de itens deve estar vazia
        Assert.Equal(0, result.TotalCount); // O total deve ser zero
    }

    [Fact]
    public async Task UpdateAsync_ReturnProviderDto()
    {
        // Arrange
        int providerId = 7;
        var providerForUpdateDto = new ProviderForUpdateDTO { Name = "Test Atualizado", Description = "Test Atualizado"};
        var providerEntity = new Provider
        {
            Id = providerId,
            Name = "Test",
            Description = "Test",
        };
        var expectedReturnDto = new ProviderDto { Id = providerId, Name = "Test Atualizado", Description = "Test Atualizada" };

        _mockProviderRepository
            .Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()))
            .ReturnsAsync(providerEntity);

        _mockProviderRepository
            .Setup(repo => repo.Update(It.IsAny<Provider>()))
            .Returns(providerEntity);

        _mockUnitOfWork
            .Setup(uof => uof.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMapper
            .Setup(mapper => mapper.Map<ProviderDto>(providerEntity))
            .Returns(expectedReturnDto);

        // Act
        var result = await _providerService.UpdateAsync(providerForUpdateDto, providerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedReturnDto.Id, result.Id);
        Assert.Equal(expectedReturnDto.Name, result.Name);
        Assert.Equal(expectedReturnDto.Description, result.Description);

        _mockProviderRepository.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()), Times.Once);
        _mockProviderRepository.Verify(repo => repo.Update(It.IsAny<Provider>()), Times.Once);
        _mockUnitOfWork.Verify(uof => uof.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<ProviderDto>(providerEntity), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnNull()
    {
        var providerId = 99;
        var updateDto = new ProviderForUpdateDTO { Name = "Test att" };
        _mockProviderRepository
            .Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()))
            .ReturnsAsync((Provider?)null);

        var result = await _providerService.UpdateAsync(updateDto, providerId);

        Assert.Null(result);

        _mockProviderRepository.Verify(repo => repo.Update(It.IsAny<Provider>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMapper.Verify(mapper => mapper.Map<ProviderDto>(It.IsAny<Provider>()), Times.Never);
    }
}
