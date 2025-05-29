using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging; 
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand, Unit>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<UpdateSaleCommandHandler> _logger; 

    public UpdateSaleCommandHandler(ISaleRepository saleRepository, ILogger<UpdateSaleCommandHandler> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (sale == null)
        {
            _logger.LogWarning("Sale with ID {SaleId} not found for update.", request.Id);            
            throw new NotFoundException($"Sale with ID {request.Id} not found.");
        }

        if (sale.IsCancelled)
        {
            _logger.LogWarning("Attempted to update cancelled sale {SaleId}.", request.Id);
            throw new DomainException("Cannot update a cancelled sale.");
        }

        // Armazenar dados antigos para o evento de modificação
        var oldSaleDate = sale.SaleDate;
        var oldTotalAmount = sale.TotalAmount;

        
        sale.UpdateSaleDetails(request.CustomerId, request.CustomerName, request.BranchId, request.BranchName, request.SaleDate);


        var currentItemIds = sale.Items.Select(i => i.Id).ToList();
        var requestItemIds = request.Items.Where(i => i.Id.HasValue).Select(i => i.Id.Value).ToList();

        
        foreach (var existingItem in sale.Items.ToList()) // Criar ToList() para modificar a coleção durante a iteração
        {
            if (!requestItemIds.Contains(existingItem.Id) && !existingItem.IsCancelled)
            {
                
                sale.RemoveItem(existingItem.Id);
                _logger.LogInformation("Item with ID {ItemId} removed from Sale {SaleId} during update.", existingItem.Id, sale.Id);
            }
        }

        foreach (var requestItem in request.Items)
        {
            if (requestItem.Id.HasValue)
            {
                
                var existingItem = sale.Items.FirstOrDefault(i => i.Id == requestItem.Id.Value);

                if (existingItem != null)
                {
                    // Atualiza o item existente
                    existingItem.UpdateItemDetails(
                        requestItem.ProductId,
                        requestItem.ProductName,
                        requestItem.Quantity,
                        requestItem.UnitPrice,
                        requestItem.Discount
                    );
                    _logger.LogInformation("Item with ID {ItemId} updated for Sale {SaleId}.", existingItem.Id, sale.Id);
                }
                else
                {
                   
                    _logger.LogWarning("Requested item with ID {ItemId} not found in Sale {SaleId} for update. Treating as new item if logic allows, otherwise ignored.", requestItem.Id.Value, sale.Id);
                   
                    var newItem = new SaleItem(
                        sale.Id,
                        requestItem.ProductId,
                        requestItem.ProductName,
                        requestItem.Quantity,
                        requestItem.UnitPrice,
                        requestItem.Discount
                    );
                    sale.AddItem(newItem);
                    _logger.LogInformation("New item with ID {NewItemId} added to Sale {SaleId}.", newItem.Id, sale.Id);
                }
            }
            else
            {
                
                var newItem = new SaleItem(
                    sale.Id, 
                    requestItem.ProductId,
                    requestItem.ProductName,
                    requestItem.Quantity,
                    requestItem.UnitPrice,
                    requestItem.Discount
                );
                sale.AddItem(newItem);
                _logger.LogInformation("New item with ID {NewItemId} added to Sale {SaleId}.", newItem.Id, sale.Id);
            }
        }

        
        sale.CalculateTotalAmount();

       
        await _saleRepository.UpdateAsync(sale, cancellationToken);
        _logger.LogInformation("Sale {SaleId} updated successfully.", sale.Id);
               

        return Unit.Value; 
    }
}