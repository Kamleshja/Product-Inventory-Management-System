using FluentValidation;
using PIMS.Application.DTOs.Product;

namespace PIMS.Application.Validators.Product;

public class UpdateProductPriceValidator : AbstractValidator<UpdateProductPriceDto>
{
    public UpdateProductPriceValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.NewPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500);
    }
}