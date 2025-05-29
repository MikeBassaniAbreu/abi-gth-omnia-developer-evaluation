using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleCommand, Sale>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SaleNumber, opt => opt.Ignore()) 
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsCancelled, opt => opt.Ignore());

        CreateMap<UpdateSaleCommandItem, SaleItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) 
            .ForMember(dest => dest.SaleId, opt => opt.Ignore())
            .ForMember(dest => dest.Discount, opt => opt.Ignore())
            .ForMember(dest => dest.TotalItemAmount, opt => opt.Ignore())
            .ForMember(dest => dest.IsCancelled, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}