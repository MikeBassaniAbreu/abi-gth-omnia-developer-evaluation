using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale; // Usar o Command da camada de aplicação

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Profile for mapping between API Request and Application Command for UpdateSale operation.
/// </summary>
public class UpdateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for UpdateSale feature.
    /// </summary>
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
        CreateMap<UpdateSaleItemRequest, UpdateSaleCommandItem>();
        CreateMap<Guid, UpdateSaleResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src));
    }
}