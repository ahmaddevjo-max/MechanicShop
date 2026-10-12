using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GeneratTokans
{
    public class GenerateTokenQueryValidation : AbstractValidator<GenerateTokenQuery>
    {
       
        public GenerateTokenQueryValidation()
        {
            RuleFor(request => request.Email)
             .NotNull().NotEmpty()
             .WithErrorCode("Email_Null_Or_Empty")
             .WithMessage("Email cannot be null or empty");

            RuleFor(request => request.Password)
                .NotNull().NotEmpty()
                .WithErrorCode("Password_Null_Or_Empty")
                .WithMessage("Password cannot be null or empty.");
        }

    }
}
