using MediatR;
using System;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleCommand : IRequest<Unit>
{
    public Guid Id { get; set; }

    public CancelSaleCommand(Guid id)
    {
        Id = id;
    }
}