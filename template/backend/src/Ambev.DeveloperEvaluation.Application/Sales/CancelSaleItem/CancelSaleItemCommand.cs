using MediatR;
using System;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemCommand : IRequest<Unit>
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }

    public CancelSaleItemCommand(Guid saleId, Guid itemId)
    {
        SaleId = saleId;
        ItemId = itemId;
    }
}