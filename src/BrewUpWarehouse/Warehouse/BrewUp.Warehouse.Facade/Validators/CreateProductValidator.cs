using BrewUp.Shared.ExternalContracts;
using FluentValidation;

namespace BrewUp.Warehouse.Facade.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductJson>
{
    public CreateProductValidator()
    {
        RuleFor(v => v.ProductName).NotEmpty().NotNull();
        RuleFor(v => v.ProductDescription).NotEmpty().NotNull();
        RuleFor(v => v.ProductType).NotEmpty().NotNull();
    }
}