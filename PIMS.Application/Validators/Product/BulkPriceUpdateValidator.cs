using FluentValidation;
using PIMS.Application.DTOs.Product;

namespace PIMS.Application.Validators.Product;

public class BulkPriceUpdateValidator : AbstractValidator<BulkPriceUpdateDto>
{
    public BulkPriceUpdateValidator()
    {
        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("At least one product must be selected.");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Adjustment value must be greater than zero.");

        RuleFor(x => x.AdjustmentType)
            .Must(x => x == "Percentage" || x == "Fixed")
            .WithMessage("AdjustmentType must be 'Percentage' or 'Fixed'.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500);
    }
}