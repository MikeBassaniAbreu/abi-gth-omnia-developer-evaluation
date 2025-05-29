using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSaleDetails;

/// <summary>
/// Profile for mapping between Sale entities and GetSaleDetailsQueryResult/Item
/// </summary>
public class GetSaleDetailsProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetSaleDetails operation
    /// </summary>
    public GetSaleDetailsProfile()
    {
        CreateMap<Sale, GetSaleDetailsQueryResult>();
        CreateMap<SaleItem, GetSaleDetailsQueryResultItem>();
    }
}