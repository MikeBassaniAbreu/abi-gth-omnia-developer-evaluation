using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Guid>
{
    private readonly ISaleRepository _saleRepository;

    public CreateSaleCommandHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var saleNumberExists = await _saleRepository.SaleNumberExistsAsync(request.SaleNumber, cancellationToken);
        if (saleNumberExists)
        {
            throw new DomainException($"Sale with number '{request.SaleNumber}' already exists.");
        }

        var sale = new Sale(
            request.SaleNumber,
            request.SaleDate,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName
        );

        foreach (var itemCommand in request.Items)
        {
            var saleItem = new SaleItem(
                itemCommand.ProductId,
                itemCommand.ProductName,
                itemCommand.Quantity,
                itemCommand.UnitPrice
            );

            sale.AddItem(saleItem);
        }

        await _saleRepository.AddAsync(sale, cancellationToken);

        return sale.Id;
    }
}