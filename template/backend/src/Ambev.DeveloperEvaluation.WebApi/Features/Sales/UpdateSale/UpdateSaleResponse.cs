using System;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// API response model for UpdateSale operation.
/// </summary>
public class UpdateSaleResponse
{
    /// <summary>
    /// The unique identifier of the updated sale.
    /// </summary>
    public Guid Id { get; set; }
}