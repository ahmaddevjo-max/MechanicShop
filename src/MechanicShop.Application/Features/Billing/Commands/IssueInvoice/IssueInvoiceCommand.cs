
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice
{
    public record IssueInvoiceCommand(Guid WorkOrderId) : IRequest<Result<InvoiceDto>>;
 
}
