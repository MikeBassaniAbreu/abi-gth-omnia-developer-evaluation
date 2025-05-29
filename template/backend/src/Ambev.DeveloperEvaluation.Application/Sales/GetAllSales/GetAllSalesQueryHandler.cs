using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using AutoMapper; 
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, IEnumerable<GetAllSalesQueryResult>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper; 

    public GetAllSalesQueryHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GetAllSalesQueryResult>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {        
        var sales = await _saleRepository.GetAllAsync(cancellationToken);
        var result = _mapper.Map<IEnumerable<GetAllSalesQueryResult>>(sales);

        return result;
    }
}