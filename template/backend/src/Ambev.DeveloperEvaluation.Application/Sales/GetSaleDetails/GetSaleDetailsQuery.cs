using MediatR;
using System;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSaleDetails;

public class GetSaleDetailsQuery : IRequest<GetSaleDetailsQueryResult?>
{
    public Guid Id { get; set; }

    public GetSaleDetailsQuery(Guid id)
    {
        Id = id;
    }
}

public class GetSaleDetailsQueryResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<GetSaleDetailsQueryResultItem> Items { get; set; } = new List<GetSaleDetailsQueryResultItem>();
}

public class GetSaleDetailsQueryResultItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalItemAmount { get; set; }
    public bool IsCancelled { get; set; }
}