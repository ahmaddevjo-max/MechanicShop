
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Constants;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOerders.Billing;
using MechanicShop.Domain.WorkOerders.Enums;
using MechanicShop.Domain.Workorders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice
{
    public class IssueInvoiceCommandHandler(IAppDbContext context , ILogger<IssueInvoiceCommandHandler> logger ,TimeProvider datetime, HybridCache cache) 
        : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
    {
        public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand command, CancellationToken ct)
        {


            var workOrder = await context.WorkOrders
               .Include(w => w.Vehicle!)
                   .ThenInclude(v => v.Customer)
             .Include(w => w.RepairTasks)
               .ThenInclude(rt => rt.Parts)
             .FirstOrDefaultAsync(w => w.Id == command.WorkOrderId, ct);

            if (workOrder is null)
            {
                logger.LogWarning("Invoice issuance failed. WorkOrder {WorkOrderId} not found.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }


            if (workOrder.State != WorkOrderState.Completed)
            {
                logger.LogWarning("Invoice issuance rejected. WorkOrder {WorkOrderId} is not in completed.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
            }

            var lineItems = new List<InvoiceLineItem>();
            Guid invoiceId = Guid.NewGuid(); 
            var lineNumber = 1;


            foreach (var (task, taskIndex) in workOrder.RepairTasks.Select((t, i) => (t, i + 1)))
            {

                 var partsSummary = task.Parts.Any()  ?
                 string.Join(Environment.NewLine , task.Parts.Select(p => $"    • {p.Name} x{p.Quantity} @ {p.Cost:C}"))
                 : "    • No parts";



                var lineDescription =
                         $"{taskIndex}: {task.Name}{Environment.NewLine}" +
                         $"  Labor = {task.LaborCost:C}{Environment.NewLine}" +
                         $"  Parts:{Environment.NewLine}{partsSummary}";



                var totalPartsCost = task.Parts.Sum(p => p.Cost * p.Quantity);
                var totalTaskCost = task.LaborCost + totalPartsCost;


                var lineItemResult = InvoiceLineItem.Create(
                   invoiceId,
                   lineNumber++,
                   lineDescription,
                   1,
                   totalTaskCost);
               
                
                if (lineItemResult.IsError)
                {
                    return lineItemResult.Errors;
                }

                lineItems.Add(lineItemResult.Value);


            }

            var subTotal = lineItems.Sum(l => l.UnitPrice);
            var taxAmount = subTotal * MechanicShopConstants.TaxRate;
            var DesCountAmount= workOrder.Discount ?? 0;


            var InvoiceResult = Invoice.Create(
                invoiceId,
                workOrder.Id,
                lineItems,
                DesCountAmount,
                taxAmount,
                datetime
                );


            if(InvoiceResult.IsError)
            {
                return InvoiceResult.Errors;
            }


            var invoice = InvoiceResult.Value;

           await context.Invoices.AddAsync(invoice,ct);

            await context.SaveChangesAsync(ct);

            await cache.RemoveByTagAsync("invoice",ct);
            logger.LogInformation("Invoice {InvoiceId} issued for WorkOrder {WorkOrderId}.", invoice.Id, workOrder.Id);

            return invoice.ToDto();
        }
    }
}
