using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSaleDetails;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
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
    /// <returns>A list of sales.</returns
    [HttpGet] 
    [ProducesResponseType(typeof(IEnumerable<GetAllSalesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllSales()
    {        
        var query = new GetAllSalesQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a sale by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <returns>The requested sale.</returns>
    [HttpGet("{id}")] 
    [ProducesResponseType(typeof(GetSaleDetailsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSaleById([FromRoute] Guid id)
    {
        
        var query = new GetSaleDetailsQuery(id);

        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Sale with ID {id} not found.");
        }

        return Ok(result);
    }


    /// <summary>
    /// Updates an existing sale.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to update.</param>
    /// <param name="request">The sale update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the updated sale if successful.</returns>    ///    
    [HttpPut("{id}")] 
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        
        var validator = new UpdateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => (ValidationErrorDetail)e).ToList();
            return BadRequest(new ApiResponse { Success = false, Message = "Validation failed.", Errors = errors });
        }

       
        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id; 

        try
        {
            
            await _mediator.Send(command, cancellationToken);

            
            return Ok(new ApiResponseWithData<UpdateSaleResponse>
            {
                Success = true,
                Message = "Sale updated successfully.",
                Data = new UpdateSaleResponse { Id = id }
            });
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            
            if (ex.Message.Contains("not found")) 
            {
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Success = false, Message = "An unexpected error occurred." });
        }
    }


}