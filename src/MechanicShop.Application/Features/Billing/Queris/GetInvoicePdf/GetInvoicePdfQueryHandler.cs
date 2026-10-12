using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MechanicShop.Application.Features.Billing.Queris.GetInvoicePdf
{
    public class GetInvoicePdfQueryHandler(IAppDbContext context 
        , ILogger<GetInvoicePdfQueryHandler> logger 
        , IInvoicePdfGenerator invoicePdfGenerator)
        : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
    {
        public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery query, CancellationToken ct)
        {

            var invoice = await context.Invoices.AsNoTracking()
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == query.InvoiceId , ct);
                                            

            if(invoice is null)
            {
                logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", query.InvoiceId);
                return ApplicationErrors.InvoiceNotFound;
            }

            try
            {
                var pdfBytes = invoicePdfGenerator.Generate(invoice);
                var invoicePdf = new InvoicePdfDto
                {
                    Content = pdfBytes,
                    FileName = $"invoice-{invoice.Id}.pdf"
                };

                return invoicePdf;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate PDF for InvoiceId: {InvoiceId}", query.InvoiceId);
                return Error.Failure("An error occurred while generating the invoice PDF.");
            }




        }
    }
}
