using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

/// <summary>
/// Profile for mapping between Sale entities and GetAllSalesQueryResult/Item
/// </summary>
public class GetAllSalesProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetAllSales operation
    /// </summary>
    public GetAllSalesProfile()
    {
        CreateMap<Sale, GetAllSalesQueryResult>();
        CreateMap<SaleItem, GetAllSalesQueryResultItem>();
    }
}