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

namespace Ambev.DeveloperEvaluation.UnitTests.Application; // Adapte o namespace se necessário

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
        var item1ProductId = Guid.NewGuid(); // <-- Renomeado para clareza
        var item2ProductId = Guid.NewGuid(); // <-- Renomeado para clareza
        var sale = new Sale("VENDA-001", DateTime.UtcNow, Guid.NewGuid(), "Customer A", Guid.NewGuid(), "Branch A");
        // Adiciona itens usando os ProductIds
        sale.AddItem(new SaleItem(item1ProductId, "Produto A", 2, 10.0m));
        sale.AddItem(new SaleItem(item2ProductId, "Produto B", 5, 5.0m));

        // Total inicial da venda: 42.5m (20.0m + 22.5m)
        Assert.Equal(42.5m, sale.TotalAmount);
        // CORRIGIDO: Procurar pelo ProductId, não pelo Id do SaleItem
        Assert.False(sale.Items.First(i => i.ProductId == item1ProductId).IsCancelled); // <-- CORRIGIDO AQUI!

        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(sale);
        _mockSaleRepository.Setup(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

        // O comando de cancelar item usa o ID do ITEM da venda, não o ProductId
        // Precisamos capturar o Id real do SaleItem que foi adicionado
        // Para fazer isso, usaremos o SaleItem.Id que foi gerado
        var actualItem1Id = sale.Items.First(i => i.ProductId == item1ProductId).Id; // <-- CAPTURA O ID REAL DO ITEM
        var command = new CancelSaleItemCommand(saleId, actualItem1Id); // <-- Usa o ID REAL do SaleItem
        var cancellationToken = CancellationToken.None;

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        // CORRIGIDO: Procurar pelo ProductId para verificar o cancelamento
        Assert.True(sale.Items.First(i => i.ProductId == item1ProductId).IsCancelled); // Item 1 deve estar cancelado
        Assert.False(sale.Items.First(i => i.ProductId == item2ProductId).IsCancelled); // Item 2 não deve estar cancelado
        Assert.Equal(22.5m, sale.TotalAmount); // Total deve ser recalculado (apenas item2 restando, 22.5m)

        _mockSaleRepository.Verify(r => r.UpdateAsync(sale, It.IsAny<CancellationToken>()), Times.Once());

    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenSaleNotFound()
    {
        // Arrange
        var nonExistentSaleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        _mockSaleRepository.Setup(r => r.GetByIdAsync(nonExistentSaleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Sale)null); // Retorna null

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
        sale.AddItem(new SaleItem(Guid.NewGuid(), "Existing Product", 1, 10.0m)); // Adiciona um item diferente

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
        var productId = Guid.NewGuid(); // <-- Renomeado para clareza: este é o ID do Produto
        var sale = new Sale("VENDA-003", DateTime.UtcNow, Guid.NewGuid(), "Customer C", Guid.NewGuid(), "Branch C");

        // Cria o SaleItem passando o productId
        var itemToCancel = new SaleItem(productId, "Produto C", 1, 20.0m);
        itemToCancel.Cancel(); // Marcar o item como cancelado ANTES de adicionar à venda (simulando estado pré-existente)
        sale.AddItem(itemToCancel); // Adiciona o item à venda

        // CAPTURA O ID REAL do SaleItem que foi adicionado à venda
        var actualSaleItemId = sale.Items.First(i => i.ProductId == productId).Id; // <-- AQUI!

        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(sale); // Retorna a venda com o item já cancelado

        // Cria o comando usando o ID REAL do SaleItem
        var command = new CancelSaleItemCommand(saleId, actualSaleItemId); // <-- AQUI!
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, cancellationToken));

        Assert.Equal("Sale item is already cancelled.", exception.Message); // Verifica a mensagem da exceção
                                                                            // Verifica que o UpdateAsync NUNCA foi chamado, pois a exceção deve impedir
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