using FluentValidation;
using System;
using System.Linq;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleRequest that defines validation rules for sale update.
/// </summary>
public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    /// <summary>
    /// Initializes a new instance of the UpdateSaleRequestValidator with defined validation rules.
    /// </summary>
    public UpdateSaleRequestValidator()
    {
        RuleFor(x => x.SaleDate)
            .NotEmpty().WithMessage("Sale date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Sale date cannot be in the future.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(255).WithMessage("Customer name cannot exceed 255 characters.");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(255).WithMessage("Branch name cannot exceed 255 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sale must have at least one item.")
            .Must(items => items != null && items.Any()).WithMessage("Sale must have at least one item.")
            .ForEach(item =>
            {
                item.SetValidator(new UpdateSaleItemRequestValidator());
            });
    }
}

/// <summary>
/// Validator for UpdateSaleItemRequest that defines validation rules for sale item update.
/// </summary>
public class UpdateSaleItemRequestValidator : AbstractValidator<UpdateSaleItemRequest>
{
    /// <summary>
    /// Initializes a new instance of the UpdateSaleItemRequestValidator with defined validation rules.
    /// </summary>
    public UpdateSaleItemRequestValidator()
    {
        // O ID pode ser nulo para novos itens, então não há regra NotEmpty para ele aqui.

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required for a sale item.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required for a sale item.")
            .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(20).WithMessage("It's not possible to sell above 20 identical items.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.");
    }
}