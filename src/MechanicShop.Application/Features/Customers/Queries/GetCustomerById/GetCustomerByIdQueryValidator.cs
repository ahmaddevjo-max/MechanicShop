using FluentValidation;
namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
    {
        GetCustomerByIdQueryValidator()
        {
            RuleFor(request => request.CustomerId)
                              .NotEmpty()
                              .WithErrorCode("CustomerId_Is_Required")
                              .WithMessage("CustomerId is required.");
        }
    }
}
