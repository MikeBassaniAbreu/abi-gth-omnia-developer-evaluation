using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Application;

public class CancelSaleItemCommandHandlerTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepository;
    private readonly Mock<ILogger<CancelSaleItemCommandHandler>> _mockLogger;
    private readonly CancelSaleItemCommandHandler _handler;

    public CancelSaleItemCommandHandlerTests()
    {
        _mockSaleRepository = new Mock<ISaleRepository>();
        _mockLogger = new Mock<ILogger<CancelSaleItemCommandHandler>>();
        _handler = new CancelSaleItemCommandHandler(_mockSaleRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelSaleItemSuccessfully_WhenSaleAndItemExist()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var item1ProductId = Guid.NewGuid(); 
        var item2ProductId = Guid.NewGuid(); 
        var sale = new Sale("VENDA-001", DateTime.UtcNow, Guid.NewGuid(), "Customer A", Guid.NewGuid(), "Branch A");
        // Adiciona itens usando os ProductIds
        sale.AddItem(new SaleItem(item1ProductId, "Produto A", 2, 10.0m));
        sale.AddItem(new SaleItem(item2ProductId, "Produto B", 5, 5.0m));

        
        Assert.Equal(42.5m, sale.TotalAmount);
        
        Assert.False(sale.Items.First(i => i.ProductId == item1ProductId).IsCancelled); 

        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(sale);
        _mockSaleRepository.Setup(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

       
        var actualItem1Id = sale.Items.First(i => i.ProductId == item1ProductId).Id; 
        var command = new CancelSaleItemCommand(saleId, actualItem1Id); 
        var cancellationToken = CancellationToken.None;

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert        
        Assert.True(sale.Items.First(i => i.ProductId == item1ProductId).IsCancelled); 
        Assert.False(sale.Items.First(i => i.ProductId == item2ProductId).IsCancelled); 
        Assert.Equal(22.5m, sale.TotalAmount); 

        _mockSaleRepository.Verify(r => r.UpdateAsync(sale, It.IsAny<CancellationToken>()), Times.Once());

    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleNotFound()
    {
        // Arrange
        var nonExistentSaleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        _mockSaleRepository.Setup(r => r.GetByIdAsync(nonExistentSaleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Sale)null); 

        var command = new CancelSaleItemCommand(nonExistentSaleId, itemId);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal($"Sale with ID {nonExistentSaleId} not found.", exception.Message);
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleItemNotFoundInSale()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var nonExistentItemId = Guid.NewGuid();
        var sale = new Sale("VENDA-002", DateTime.UtcNow, Guid.NewGuid(), "Customer B", Guid.NewGuid(), "Branch B");
        sale.AddItem(new SaleItem(Guid.NewGuid(), "Existing Product", 1, 10.0m)); 

        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(sale);

        var command = new CancelSaleItemCommand(saleId, nonExistentItemId);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal($"Sale item with ID {nonExistentItemId} not found in this sale.", exception.Message);
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleItemIsAlreadyCancelled()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid(); 
        var sale = new Sale("VENDA-003", DateTime.UtcNow, Guid.NewGuid(), "Customer C", Guid.NewGuid(), "Branch C");

        
        var itemToCancel = new SaleItem(productId, "Produto C", 1, 20.0m);
        itemToCancel.Cancel(); 
        sale.AddItem(itemToCancel); 

        
        var actualSaleItemId = sale.Items.First(i => i.ProductId == productId).Id;

        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(sale); 

        
        var command = new CancelSaleItemCommand(saleId, actualSaleItemId); 
        var cancellationToken = CancellationToken.None;

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal("Sale item is already cancelled.", exception.Message); 
                                                                            
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());

        
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleIsAlreadyCancelled()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var sale = new Sale("VENDA-004", DateTime.UtcNow, Guid.NewGuid(), "Customer D", Guid.NewGuid(), "Branch D");
        sale.Cancel(); // Marcar a VENDA como cancelada
        sale.AddItem(new SaleItem(itemId, "Produto D", 1, 30.0m));

        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(sale);

        var command = new CancelSaleItemCommand(saleId, itemId);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal("Cannot cancel an item in a cancelled sale.", exception.Message);
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}