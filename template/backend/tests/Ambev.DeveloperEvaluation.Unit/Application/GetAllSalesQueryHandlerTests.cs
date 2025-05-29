using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Application;

public class GetAllSalesQueryHandlerTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetAllSalesQueryHandler _handler;

    public GetAllSalesQueryHandlerTests()
    {
        _mockSaleRepository = new Mock<ISaleRepository>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetAllSalesQueryHandler(_mockSaleRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllSales_WhenSalesExist()
    {
        // Arrange
        var salesFromRepo = new List<Sale>
        {
            new Sale("VENDA-001", DateTime.Now, Guid.NewGuid(), "Cliente A", Guid.NewGuid(), "Filial X"),
            new Sale("VENDA-002", DateTime.Now, Guid.NewGuid(), "Cliente B", Guid.NewGuid(), "Filial Y")
        };
        var mappedResults = new List<GetAllSalesQueryResult>
        {
            new GetAllSalesQueryResult { Id = salesFromRepo[0].Id, CustomerName = salesFromRepo[0].CustomerName, SaleDate = salesFromRepo[0].SaleDate },
            new GetAllSalesQueryResult { Id = salesFromRepo[1].Id, CustomerName = salesFromRepo[1].CustomerName, SaleDate = salesFromRepo[1].SaleDate }
        };

        _mockSaleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(salesFromRepo);
        _mockMapper.Setup(m => m.Map<IEnumerable<GetAllSalesQueryResult>>(It.Is<IEnumerable<Sale>>(s => s.Count() == salesFromRepo.Count)))
                   .Returns(mappedResults);

        var query = new GetAllSalesQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mappedResults.Count, result.Count());
        Assert.Contains(result, r => r.Id == mappedResults[0].Id);
        Assert.Contains(result, r => r.Id == mappedResults[1].Id);

        _mockSaleRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<GetAllSalesQueryResult>>(It.IsAny<IEnumerable<Sale>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSalesExist()
    {
        // Arrange
        var salesFromRepo = new List<Sale>();
        var mappedResults = new List<GetAllSalesQueryResult>(); 

        _mockSaleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(salesFromRepo);
        _mockMapper.Setup(m => m.Map<IEnumerable<GetAllSalesQueryResult>>(It.IsAny<IEnumerable<Sale>>()))
                   .Returns(mappedResults);

        var query = new GetAllSalesQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockSaleRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<GetAllSalesQueryResult>>(It.IsAny<IEnumerable<Sale>>()), Times.Once);
    }
}