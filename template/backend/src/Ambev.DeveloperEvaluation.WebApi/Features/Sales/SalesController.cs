using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSaleDetails;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using AutoMapper;
using FluentValidation.Results; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; 

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Controller for managing sales operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale.
    /// </summary>
    /// <param name="request">The sale creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created sale details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => (ValidationErrorDetail)e).ToList();
            return BadRequest(new ApiResponse { Success = false, Message = "Validation failed.", Errors = errors });
        }
        
        var command = _mapper.Map<CreateSaleCommand>(request);
        
        var saleId = await _mediator.Send(command, cancellationToken);

        var responseData = _mapper.Map<CreateSaleResponse>(saleId);
        
        return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale created successfully.",
            Data = responseData
        });
    }



    /// <summary>
    /// Retrieves a list of all sales.
    /// </summary>
    /// <returns>A list of sales.</returns>
    /// <response code="200">Returns the list of sales.</response>
    /// <response code="500">If an unhandled error occurs.</response>
    [HttpGet] // Define este método como um endpoint HTTP GET
    [ProducesResponseType(typeof(IEnumerable<GetAllSalesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllSales()
    {
        // Cria uma nova instância da query para obter todas as vendas
        var query = new GetAllSalesQuery();

        // Envia a query para o MediatR e aguarda o resultado
        var result = await _mediator.Send(query);

        // Retorna um OK (200) com a lista de vendas
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a sale by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <returns>The requested sale.</returns>
    /// <response code="200">Returns the requested sale.</response>
    /// <response code="404">If the sale is not found.</response>
    /// <response code="500">If an unhandled error occurs.</response>
    [HttpGet("{id}")] // Define este método como um endpoint HTTP GET com um parâmetro de rota 'id'
    [ProducesResponseType(typeof(GetSaleDetailsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSaleById([FromRoute] Guid id) // [FromRoute] é opcional, mas boa prática para clareza
    {
        // Cria uma nova instância da query com o ID recebido da rota
        var query = new GetSaleDetailsQuery(id);

        // Envia a query para o MediatR e aguarda o resultado
        var result = await _mediator.Send(query);

        // Se o resultado for null (venda não encontrada), retorna 404 Not Found
        if (result == null)
        {
            return NotFound($"Sale with ID {id} not found.");
        }

        // Retorna um OK (200) com os detalhes da venda
        return Ok(result);
    }


}