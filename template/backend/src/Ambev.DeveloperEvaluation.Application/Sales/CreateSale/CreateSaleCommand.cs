using MediatR;
using System;
using System.Collections.Generic;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleCommand : IRequest<Guid>
{
    public string   SaleNumber      { get; set; } = string.Empty;
    public DateTime SaleDate        { get; set; }
    public Guid     CustomerId      { get; set; }
    public string   CustomerName    { get; set; } = string.Empty;
    public Guid     BranchId        { get; set; }
    public string   BranchName      { get; set; } = string.Empty;
    public List<CreateSaleItemCommand> Items { get; set; } = new List<CreateSaleItemCommand>();
}
public class CreateSaleItemCommand
{
    public Guid     ProductId   { get; set; }
    public string   ProductName { get; set; } = string.Empty;
    public int      Quantity    { get; set; }
    public decimal  UnitPrice   { get; set; }
}