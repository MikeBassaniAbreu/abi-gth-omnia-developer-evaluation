using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using System;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Domain.Entities; // Adapte o namespace se necessário

public class SaleItemTests
{
    [Fact]
    public void SaleItem_Constructor_ShouldCreateItemWithValidDataAndCalculateTotal()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Produto Teste";
        var quantity = 5;
        var unitPrice = 10.0m;

        // Act
        var item = new SaleItem(productId, productName, quantity, unitPrice);

        // Assert
        Assert.NotNull(item);
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(productName, item.ProductName);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
        Assert.Equal(5.0m, item.Discount);
        Assert.Equal(45.0m, item.TotalItemAmount); 
        Assert.False(item.IsCancelled);
        Assert.NotNull(item.CreatedAt);
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public void SaleItem_ConstructorWithSaleIdAndDiscount_ShouldCreateItemCorrectly()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productName = "Produto Com Desconto";
        var quantity = 10;
        var unitPrice = 10.0m;
        var discount = 10.0m; // Desconto fixo para teste

        // Act
        var item = new SaleItem(saleId, productId, productName, quantity, unitPrice, discount);

        // Assert
        Assert.Equal(saleId, item.SaleId);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(productName, item.ProductName);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
        Assert.Equal(discount, item.Discount);
        Assert.Equal(90.0m, item.TotalItemAmount); // (10 * 10.0) - 10.0
        Assert.False(item.IsCancelled);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Prod", 1, 10.0, "Product ID cannot be empty.")]
    [InlineData("b2c4d6e8-f0a1-4b3c-5d6e-7f8a9b0c1d2e", null, 1, 10.0, "Product name cannot be null or empty.")]
    [InlineData("b2c4d6e8-f0a1-4b3c-5d6e-7f8a9b0c1d2e", "Prod", 1, 0.0, "Unit price must be greater than zero.")]
    public void SaleItem_Constructor_ShouldThrowDomainExceptionForInvalidData(
        string productIdString, string productName, int quantity, decimal unitPrice, string expectedErrorMessage)
    {
        // Arrange
        Guid productId = Guid.Parse(productIdString);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
        {
            new SaleItem(productId, productName, quantity, unitPrice);
        });

        Assert.Equal(expectedErrorMessage, exception.Message);
    }

    [Fact]
    public void UpdateQuantity_ShouldUpdateQuantityApplyDiscountAndRecalculateTotal()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), "Produto X", 2, 10.0m); // Total 20.0, Disc 0
        Assert.Equal(0m, item.Discount);
        Assert.Equal(20.0m, item.TotalItemAmount);

        // Act (Aumentar para 5, aplica 10% de desconto)
        item.UpdateQuantity(5); // 5 * 10 = 50. Desconto 10% = 5.0. Total = 45.0

        // Assert
        Assert.Equal(5, item.Quantity);
        Assert.Equal(5.0m, item.Discount); // 10% de 50.0
        Assert.Equal(45.0m, item.TotalItemAmount);
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public void UpdateQuantity_ShouldApply20PercentDiscountFor10To20Items()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), "Produto Y", 5, 10.0m); // Inicial: 50.0, Desc 5.0, Total 45.0

        // Act (Aumentar para 12, aplica 20% de desconto)
        item.UpdateQuantity(12); // 12 * 10 = 120. Desconto 20% = 24.0. Total = 96.0

        // Assert
        Assert.Equal(12, item.Quantity);
        Assert.Equal(24.0m, item.Discount); // 20% de 120.0
        Assert.Equal(96.0m, item.TotalItemAmount);
    }

    [Theory]
    [InlineData(0, "Quantity must be greater than zero.")]
    [InlineData(-1, "Quantity must be greater than zero.")]
    [InlineData(21, "It's not possible to sell above 20 identical items.")]
    public void UpdateQuantity_ShouldThrowDomainExceptionForInvalidQuantity(int invalidQuantity, string expectedErrorMessage)
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), "Prod", 1, 10.0m);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => item.UpdateQuantity(invalidQuantity));
        Assert.Equal(expectedErrorMessage, exception.Message);
    }

    [Fact]
    public void UpdateItemDetails_ShouldUpdateAllFieldsAndRecalculateTotal()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), "Prod A", 2, 10.0m); // 20.0
        var newProductId = Guid.NewGuid();
        var newProductName = "Prod B";
        var newQuantity = 3;
        var newUnitPrice = 15.0m;
        var newDiscount = 0m; // Será recalculado

        // Act
        item.UpdateItemDetails(newProductId, newProductName, newQuantity, newUnitPrice, newDiscount); // newQuantity = 3, newUnitPrice = 15.0 -> 45.0

        // Assert
        Assert.Equal(newProductId, item.ProductId);
        Assert.Equal(newProductName, item.ProductName);
        Assert.Equal(newQuantity, item.Quantity);
        Assert.Equal(newUnitPrice, item.UnitPrice);
        Assert.Equal(0m, item.Discount); // 3 itens, nenhum desconto
        Assert.Equal(45.0m, item.TotalItemAmount); // 3 * 15.0
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public void SetSaleId_ShouldSetSaleIdCorrectly()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), "Prod", 1, 10.0m);
        var saleId = Guid.NewGuid(); 
        item.SetSaleId(saleId);

        // Assert
        Assert.Equal(saleId, item.SaleId);
    }

    [Fact]
    public void Cancel_ShouldSetIsCancelledToTrue()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), "Prod", 1, 10.0m);
        Assert.False(item.IsCancelled);

        // Act
        item.Cancel();

        // Assert
        Assert.True(item.IsCancelled);
    }

    [Fact]
    public void Cancel_ShouldThrowDomainExceptionIfAlreadyCancelled()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), "Prod", 1, 10.0m);
        item.Cancel(); // Cancela uma vez

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => item.Cancel());
        Assert.Equal("Sale item is already cancelled.", exception.Message);
    }
}