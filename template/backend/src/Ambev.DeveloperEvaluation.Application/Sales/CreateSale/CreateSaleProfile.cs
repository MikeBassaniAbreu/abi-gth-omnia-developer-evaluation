using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Profile for mapping between Sale entities and CreateSaleCommand/Result
/// </summary>
public class CreateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateSale operation
    /// </summary>
    public CreateSaleProfile()
    {
       
        CreateMap<CreateSaleCommand, Sale>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) 
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) 
            .ForMember(dest => dest.Items, opt => opt.Ignore()) 
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) 
            .ForMember(dest => dest.IsCancelled, opt => opt.Ignore());

        CreateMap<CreateSaleItemCommand, SaleItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) 
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) 
            .ForMember(dest => dest.SaleId, opt => opt.Ignore()) 
            .ForMember(dest => dest.Discount, opt => opt.Ignore())
            .ForMember(dest => dest.TotalItemAmount, opt => opt.Ignore()) 
            .ForMember(dest => dest.IsCancelled, opt => opt.Ignore());
    }
}