using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemCommandHandler : IRequestHandler<CancelSaleItemCommand, Unit>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleItemCommandHandler> _logger;

    public CancelSaleItemCommandHandler(ISaleRepository saleRepository, ILogger<CancelSaleItemCommandHandler> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);

        if (sale == null)
        {
            _logger.LogWarning("Attempted to cancel item {ItemId} in sale {SaleId} but sale was not found.", request.ItemId, request.SaleId);
            throw new DomainException($"Sale with ID {request.SaleId} not found.");
        }

        
        sale.CancelItem(request.ItemId);

        
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        _logger.LogInformation("Sale item {ItemId} successfully cancelled in sale {SaleId}.", request.ItemId, request.SaleId);

        
        return Unit.Value; 
    }
}