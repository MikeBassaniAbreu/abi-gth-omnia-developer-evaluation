using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Application; // Adapte o namespace se necessário

public class CreateSaleCommandHandlerTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepository;
    private readonly Mock<ILogger<CreateSaleCommandHandler>> _mockLogger;
    private readonly CreateSaleCommandHandler _handler;

    public CreateSaleCommandHandlerTests()
    {
        _mockSaleRepository = new Mock<ISaleRepository>();
        _mockLogger = new Mock<ILogger<CreateSaleCommandHandler>>();
        _handler = new CreateSaleCommandHandler(_mockSaleRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateSaleSuccessfully_WithValidData()
    {
        // Arrange
        var command = new CreateSaleCommand 
        {
            SaleNumber = "VENDA-001",
            SaleDate = DateTime.Today,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cliente Teste",
            BranchId = Guid.NewGuid(),
            BranchName = "Filial Teste",
            Items = new List<CreateSaleItemCommand>
        {
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto A",
                Quantity = 2,
                UnitPrice = 10.0m
            },
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto B",
                Quantity = 5,
                UnitPrice = 5.0m
            }
        }
        };

        // Captura a instância de Sale que será passada para AddAsync para verificações detalhadas
        Sale capturedSale = null;
        _mockSaleRepository.Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                           .Callback<Sale, CancellationToken>((s, ct) => capturedSale = s) // Captura a Sale
                           .Returns(Task.CompletedTask);

        var cancellationToken = CancellationToken.None;

        // Act
        var resultSaleId = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(resultSaleId); 
        Assert.NotEqual(Guid.Empty, resultSaleId); 
        
        _mockSaleRepository.Verify(r => r.AddAsync(
            It.Is<Sale>(s => s.Id == resultSaleId), 
            It.IsAny<CancellationToken>()), Times.Once());

        
        Assert.NotNull(capturedSale);
        Assert.Equal(resultSaleId, capturedSale.Id);
        Assert.Equal(command.SaleNumber, capturedSale.SaleNumber);
        Assert.Equal(command.CustomerName, capturedSale.CustomerName);
        Assert.Equal(2, capturedSale.Items.Count); 
        Assert.Equal(42.5m, capturedSale.TotalAmount); 
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleDataIsInvalid()
    {
        // Arrange
        var command = new CreateSaleCommand 
        {
            SaleNumber = null,
            SaleDate = DateTime.Today,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cliente Teste",
            BranchId = Guid.NewGuid(),
            BranchName = "Filial Teste",
            Items = new List<CreateSaleItemCommand>()
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal("Sale number cannot be null or empty.", exception.Message);

        _mockSaleRepository.Verify(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleItemDataIsInvalid()
    {
        // Arrange
        var command = new CreateSaleCommand 
        {
            SaleNumber = "VENDA-002",
            SaleDate = DateTime.Today,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cliente Teste",
            BranchId = Guid.NewGuid(),
            BranchName = "Filial Teste",
            Items = new List<CreateSaleItemCommand> 
            {
                new CreateSaleItemCommand 
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "", 
                    Quantity = 1,
                    UnitPrice = 10.0m
                }
            }
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal("Product name cannot be null or empty.", exception.Message);

        _mockSaleRepository.Verify(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}