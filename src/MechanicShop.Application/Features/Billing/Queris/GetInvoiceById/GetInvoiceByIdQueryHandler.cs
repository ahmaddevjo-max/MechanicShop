

using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOerders.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queris.GetInvoiceById
{
    public class GetInvoiceByIdQueryHandler(IAppDbContext context ,ILogger<GetInvoiceByIdQueryHandler> logger) : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
    {
        public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
        {
            
            var invoice = context.Invoices.AsNoTracking().
                Include(i => i.LineItems).
                Include(i => i.WorkOrder!)
                    .ThenInclude(wo=> wo.Vehicle!).
                        ThenInclude(v => v.Customer!)
                .FirstOrDefault(i => i.Id == query.InvoiceId);

            if (invoice is null)
            {
                logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", query.InvoiceId);
                return Error.NotFound("Invoice not found.");
            }

            return invoice.ToDto();


        }
    }
}
