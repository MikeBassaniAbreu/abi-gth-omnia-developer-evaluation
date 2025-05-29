using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using AutoMapper; 
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSaleDetails; 

public class GetSaleDetailsQueryHandler : IRequestHandler<GetSaleDetailsQuery, GetSaleDetailsQueryResult?>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSaleDetailsQueryHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<GetSaleDetailsQueryResult?> Handle(GetSaleDetailsQuery request, CancellationToken cancellationToken)
    {
        
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);

        
        if (sale == null)
        {
            return null;
        }

        var result = _mapper.Map<GetSaleDetailsQueryResult>(sale);

        return result;
    }
}