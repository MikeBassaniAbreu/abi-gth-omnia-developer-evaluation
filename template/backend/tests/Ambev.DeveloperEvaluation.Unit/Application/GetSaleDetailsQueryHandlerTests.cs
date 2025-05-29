using Ambev.DeveloperEvaluation.Application.Sales.GetSaleDetails;
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

namespace Ambev.DeveloperEvaluation.UnitTests.Application.Sales; // Confirme este namespace

public class GetSaleDetailsQueryHandlerTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetSaleDetailsQueryHandler _handler;

    public GetSaleDetailsQueryHandlerTests()
    {
        _mockSaleRepository = new Mock<ISaleRepository>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetSaleDetailsQueryHandler(_mockSaleRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSaleDetails_WhenSaleExists()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        
        var saleFromRepo = new Sale("VENDA-001", DateTime.Today, customerId, "Cliente Teste", branchId, "Filial Teste");

        
        var itemQuantity = 5; 
        var itemUnitPrice = 50.0m; 
        var itemDiscount = 0.0m;


        itemQuantity = 5;
        itemUnitPrice = 100.0m;
        itemDiscount = itemUnitPrice * itemQuantity * 0.10m;

        var saleItem = new SaleItem(
            saleFromRepo.Id, 
            productId,
            "Produto A",
            itemQuantity,
            itemUnitPrice,
            itemDiscount 
        );
        saleFromRepo.AddItem(saleItem); 
        saleFromRepo.CalculateTotalAmount();
        
        var mappedResult = new GetSaleDetailsQueryResult
        {
            Id = saleFromRepo.Id,
            SaleNumber = saleFromRepo.SaleNumber,
            SaleDate = saleFromRepo.SaleDate,
            CustomerId = saleFromRepo.CustomerId,
            CustomerName = saleFromRepo.CustomerName,
            BranchId = saleFromRepo.BranchId,
            BranchName = saleFromRepo.BranchName,
            TotalAmount = saleFromRepo.TotalAmount,
            IsCancelled = saleFromRepo.IsCancelled,
            CreatedAt = saleFromRepo.CreatedAt,
            UpdatedAt = saleFromRepo.UpdatedAt,
            Items = new List<GetSaleDetailsQueryResultItem>
            {
                new GetSaleDetailsQueryResultItem
                {
                    Id = saleItem.Id,
                    ProductId = saleItem.ProductId,
                    ProductName = saleItem.ProductName,
                    Quantity = saleItem.Quantity,
                    UnitPrice = saleItem.UnitPrice,
                    Discount = saleItem.Discount,
                    TotalItemAmount = saleItem.TotalItemAmount,
                    IsCancelled = saleItem.IsCancelled
                }
            }
        };

       
        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(saleFromRepo);

        
        _mockMapper.Setup(m => m.Map<GetSaleDetailsQueryResult>(It.IsAny<Sale>()))
                   .Returns(mappedResult);

        var query = new GetSaleDetailsQuery(saleId);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mappedResult.Id, result.Id);
        Assert.Equal(mappedResult.SaleNumber, result.SaleNumber);
        Assert.Equal(mappedResult.SaleDate, result.SaleDate);
        Assert.Equal(mappedResult.CustomerId, result.CustomerId);
        Assert.Equal(mappedResult.CustomerName, result.CustomerName);
        Assert.Equal(mappedResult.BranchId, result.BranchId);
        Assert.Equal(mappedResult.BranchName, result.BranchName);
        Assert.Equal(mappedResult.TotalAmount, result.TotalAmount);
        Assert.Equal(mappedResult.IsCancelled, result.IsCancelled);
        Assert.Equal(mappedResult.CreatedAt, result.CreatedAt);
        Assert.Equal(mappedResult.UpdatedAt, result.UpdatedAt);

        Assert.NotNull(result.Items);
        Assert.Single(result.Items); 
        var actualItem = result.Items.First();
        var expectedItem = mappedResult.Items.First();
        Assert.Equal(expectedItem.Id, actualItem.Id);
        Assert.Equal(expectedItem.ProductId, actualItem.ProductId);
        Assert.Equal(expectedItem.ProductName, actualItem.ProductName);
        Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
        Assert.Equal(expectedItem.UnitPrice, actualItem.UnitPrice);
        Assert.Equal(expectedItem.Discount, actualItem.Discount);
        Assert.Equal(expectedItem.TotalItemAmount, actualItem.TotalItemAmount);
        Assert.Equal(expectedItem.IsCancelled, actualItem.IsCancelled);


        
        _mockSaleRepository.Verify(r => r.GetByIdAsync(saleId, cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<GetSaleDetailsQueryResult>(It.IsAny<Sale>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenSaleDoesNotExist()
    {
        // Arrange
        var nonExistentSaleId = Guid.NewGuid();

        
        _mockSaleRepository.Setup(r => r.GetByIdAsync(nonExistentSaleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Sale)null);

        
        _mockMapper.Setup(m => m.Map<GetSaleDetailsQueryResult>(It.IsAny<Sale>()))
                   .Returns(new GetSaleDetailsQueryResult()); 

        var query = new GetSaleDetailsQuery(nonExistentSaleId);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.Null(result); // Esperamos null

        
        _mockSaleRepository.Verify(r => r.GetByIdAsync(nonExistentSaleId, cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<GetSaleDetailsQueryResult>(It.IsAny<Sale>()), Times.Never);
    }
}