using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice
{
    public class IssueInvoiceCommandValidator :AbstractValidator<IssueInvoiceCommand>
    {
        public IssueInvoiceCommandValidator()
        {

            RuleFor(request => request.WorkOrderId)
           .NotEmpty()
           .WithErrorCode("WorkOrderId_Is_Required")
           .WithMessage("WorkOrderId is required.");

        }
    }
}
