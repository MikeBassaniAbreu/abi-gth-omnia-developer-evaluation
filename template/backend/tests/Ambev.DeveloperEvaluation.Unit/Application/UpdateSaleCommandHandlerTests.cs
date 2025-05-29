using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediatR; 


namespace Ambev.DeveloperEvaluation.UnitTests.Application.Sales;

public class UpdateSaleCommandHandlerTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepository;
    private readonly Mock<ILogger<UpdateSaleCommandHandler>> _mockLogger;
    private readonly UpdateSaleCommandHandler _handler;

    public UpdateSaleCommandHandlerTests()
    {
        _mockSaleRepository = new Mock<ISaleRepository>();
        _mockLogger = new Mock<ILogger<UpdateSaleCommandHandler>>();
        _handler = new UpdateSaleCommandHandler(_mockSaleRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateSaleSuccessfully_WhenSaleExistsAndDataIsValid()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid(); 

        
        var existingSale = new Sale(
            "VENDA-EXISTENTE",
            DateTime.Today.AddDays(-1),
            Guid.NewGuid(),
            "Cliente Antigo",
            Guid.NewGuid(),
            "Filial Antiga"
        );
        var existingItem = new SaleItem(productId, "Produto X", 1, 100.0m);
        existingSale.AddItem(existingItem);
        existingSale.CalculateTotalAmount(); 

        
        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existingSale);
        _mockSaleRepository.Setup(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

        
        var newCustomerId = Guid.NewGuid();
        var newCustomerName = "Cliente Novo";
        var newBranchId = Guid.NewGuid();
        var newBranchName = "Filial Nova";
        var newSaleDate = DateTime.Today;

        var command = new UpdateSaleCommand
        {
            Id = saleId,
            CustomerId = newCustomerId,
            CustomerName = newCustomerName,
            BranchId = newBranchId,
            BranchName = newBranchName,
            SaleDate = newSaleDate,
            Items = new System.Collections.Generic.List<UpdateSaleCommandItem> 
        {
            new UpdateSaleCommandItem 
            {
                Id = existingItem.Id, 
                ProductId = existingItem.ProductId,
                ProductName = existingItem.ProductName,
                Quantity = existingItem.Quantity,
                UnitPrice = existingItem.UnitPrice,
                Discount = existingItem.Discount
            }
        }
        };
        var cancellationToken = CancellationToken.None;

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(existingSale);
        Assert.Equal(newCustomerId, existingSale.CustomerId);
        Assert.Equal(newCustomerName, existingSale.CustomerName);
        Assert.Equal(newBranchId, existingSale.BranchId);
        Assert.Equal(newBranchName, existingSale.BranchName);
        Assert.Equal(newSaleDate, existingSale.SaleDate);
        Assert.Equal("VENDA-EXISTENTE", existingSale.SaleNumber);

        
        Assert.Equal(100.0m, existingSale.TotalAmount); 

    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenSaleDoesNotExist()
    {
        // Arrange
        var nonExistentSaleId = Guid.NewGuid();
        
        _mockSaleRepository.Setup(r => r.GetByIdAsync(nonExistentSaleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Sale)null); 

        var command = new UpdateSaleCommand
        {
            Id = nonExistentSaleId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cliente Teste",
            BranchId = Guid.NewGuid(),
            BranchName = "Filial Teste",
            SaleDate = DateTime.Today
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert       
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal($"Sale with ID {nonExistentSaleId} not found.", exception.Message);

        
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());

    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenCommandDataIsInvalid()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var existingSale = new Sale(
            "VENDA-EXISTENTE",
            DateTime.Today,
            Guid.NewGuid(),
            "Cliente Antigo",
            Guid.NewGuid(),
            "Filial Antiga"
        );
        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existingSale);

       
        var command = new UpdateSaleCommand
        {
            Id = saleId, 
            CustomerId = Guid.NewGuid(),
            CustomerName = "", 
            BranchId = Guid.NewGuid(),
            BranchName = "Filial Teste",
            SaleDate = DateTime.Today
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal("Customer name cannot be null or empty.", exception.Message);

        
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleIsCancelled()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var cancelledSale = new Sale(
            "VENDA-CANCELADA",
            DateTime.Today,
            Guid.NewGuid(),
            "Cliente Cancelado",
            Guid.NewGuid(),
            "Filial Cancelada"
        );
        cancelledSale.Cancel(); 

        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(cancelledSale);

        var command = new UpdateSaleCommand
        {
            Id = saleId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cliente Novo",
            BranchId = Guid.NewGuid(),
            BranchName = "Filial Nova",
            SaleDate = DateTime.Today
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal("Cannot update a cancelled sale.", exception.Message);

        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());

    }
}