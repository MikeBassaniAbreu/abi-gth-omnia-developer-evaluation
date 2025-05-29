using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, Unit>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleCommandHandler> _logger;

    public CancelSaleCommandHandler(ISaleRepository saleRepository, ILogger<CancelSaleCommandHandler> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (sale == null)
        {
            _logger.LogWarning("Attempted to cancel sale with ID {SaleId} but it was not found.", request.Id);
            throw new DomainException($"Sale with ID {request.Id} not found.");
        }

        
        sale.Cancel();

        
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        _logger.LogInformation("Sale {SaleId} successfully cancelled.", sale.Id);

        return Unit.Value; 
    }
}