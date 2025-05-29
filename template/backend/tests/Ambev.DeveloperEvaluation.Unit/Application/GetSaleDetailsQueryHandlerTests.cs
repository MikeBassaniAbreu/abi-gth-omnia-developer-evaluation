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

        // Crie uma entidade Sale com um item para simular a venda do repositório
        var saleFromRepo = new Sale("VENDA-001", DateTime.Today, customerId, "Cliente Teste", branchId, "Filial Teste");

        // CORREÇÃO AQUI: Use o construtor completo do SaleItem
        // Passe o saleId da venda que está sendo criada e um valor para desconto (pode ser 0m se não houver desconto para essa quantidade)
        var itemQuantity = 5; // Exemplo de quantidade
        var itemUnitPrice = 50.0m; // Exemplo de preço unitário
        var itemDiscount = 0.0m; // Inicialize o desconto, ele será calculado internamente por ApplyDiscount() se a quantidade mudar

        // Se sua lógica de desconto automático é baseada na quantidade (como em ApplyDiscount),
        // você pode calcular o desconto antes de passar para o construtor se quiser que o construtor
        // use o desconto já calculado.
        // Ou, se o construtor que aceita o desconto já calcula o TotalItemAmount internamente,
        // apenas passe o desconto.
        // Pela sua entidade, o construtor que recebe 'discount' já calcula: TotalItemAmount = (UnitPrice * Quantity) - Discount;
        // Então vamos apenas passar um desconto para o construtor que simula um item já calculado.

        // Vamos simular um item que se qualifica para desconto de 10% (4 a 9 itens)
        itemQuantity = 5;
        itemUnitPrice = 100.0m;
        itemDiscount = itemUnitPrice * itemQuantity * 0.10m; // Exemplo de cálculo de 10% de desconto

        var saleItem = new SaleItem(
            saleFromRepo.Id, // Use o SaleId da Sale que ele pertence
            productId,
            "Produto A",
            itemQuantity,
            itemUnitPrice,
            itemDiscount // Passe o valor do desconto
        );
        saleFromRepo.AddItem(saleItem); // Adicione o item à venda
        saleFromRepo.CalculateTotalAmount(); // Recalcule o total da venda após adicionar o item

        // Crie o GetSaleDetailsQueryResult esperado com base na estrutura real do seu DTO
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

        // Configure o mock do repositório para retornar a venda
        _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(saleFromRepo);

        // Configure o mock do AutoMapper para retornar o resultado mapeado esperado
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
        Assert.Single(result.Items); // Deve ter 1 item
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


        // Verifique se os métodos do mock foram chamados corretamente
        _mockSaleRepository.Verify(r => r.GetByIdAsync(saleId, cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<GetSaleDetailsQueryResult>(It.IsAny<Sale>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenSaleDoesNotExist()
    {
        // Arrange
        var nonExistentSaleId = Guid.NewGuid();

        // Configure o mock do repositório para retornar null (venda não encontrada)
        _mockSaleRepository.Setup(r => r.GetByIdAsync(nonExistentSaleId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Sale)null);

        // O Mapper NÃO deve ser chamado se a venda não for encontrada
        _mockMapper.Setup(m => m.Map<GetSaleDetailsQueryResult>(It.IsAny<Sale>()))
                   .Returns(new GetSaleDetailsQueryResult()); // Retorno dummy, mas não deve ser usado

        var query = new GetSaleDetailsQuery(nonExistentSaleId);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.Null(result); // Esperamos null

        // Verifique se o método do repositório foi chamado e o do mapper NÃO foi
        _mockSaleRepository.Verify(r => r.GetByIdAsync(nonExistentSaleId, cancellationToken), Times.Once);
        _mockMapper.Verify(m => m.Map<GetSaleDetailsQueryResult>(It.IsAny<Sale>()), Times.Never);
    }
}