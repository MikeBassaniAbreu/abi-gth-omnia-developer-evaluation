using MediatR;
using System;
using System.Collections.Generic;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommand : IRequest<Unit> 
{
    
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;   
    public DateTime SaleDate { get; set; }    
    public List<UpdateSaleCommandItem> Items { get; set; } = new List<UpdateSaleCommandItem>();
}

public class UpdateSaleCommandItem
{
    
    public Guid? Id { get; set; } 
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } 
    public decimal Discount { get; set; } 
    
}