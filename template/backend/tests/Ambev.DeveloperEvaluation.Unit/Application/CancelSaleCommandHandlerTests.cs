using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Application; 

public class CancelSaleCommandHandlerTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepository;
    private readonly Mock<ILogger<CancelSaleCommandHandler>> _mockLogger;
    private readonly CancelSaleCommandHandler _handler;

    public CancelSaleCommandHandlerTests()
    {
        _mockSaleRepository = new Mock<ISaleRepository>();
        _mockLogger = new Mock<ILogger<CancelSaleCommandHandler>>();
        _handler = new CancelSaleCommandHandler(_mockSaleRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelSaleSuccessfully_WhenSaleExists()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var initialSale = new Sale("VENDA-001", DateTime.UtcNow, Guid.NewGuid(), "Customer A", Guid.NewGuid(), "Branch A");

       
        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(initialSale);

        
        _mockSaleRepository.Setup(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask); 

        var command = new CancelSaleCommand(saleId);
        var cancellationToken = CancellationToken.None;

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert       
        Assert.True(initialSale.IsCancelled);
       
        _mockSaleRepository.Verify(r => r.UpdateAsync(initialSale, It.IsAny<CancellationToken>()), Times.Once());        
        
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleNotFound()
    {
        // Arrange
        var nonExistentSaleId = Guid.NewGuid();

        
        _mockSaleRepository.Setup(r => r.GetByIdAsync(nonExistentSaleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Sale)null); // Retorna null

        var command = new CancelSaleCommand(nonExistentSaleId);
        var cancellationToken = CancellationToken.None;

        // Act & Assert        
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        
        Assert.Equal($"Sale with ID {nonExistentSaleId} not found.", exception.Message);
        
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Attempted to cancel sale with ID {nonExistentSaleId} but it was not found.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleIsAlreadyCancelled()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var alreadyCancelledSale = new Sale("VENDA-002", DateTime.UtcNow, Guid.NewGuid(), "Customer B", Guid.NewGuid(), "Branch B");
        alreadyCancelledSale.Cancel(); 

       
        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(alreadyCancelledSale);

        var command = new CancelSaleCommand(saleId);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        
        Assert.Equal("Sale is already cancelled.", exception.Message);
        
        _mockSaleRepository.Verify(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}