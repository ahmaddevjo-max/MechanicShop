using FluentValidation;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Workorders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Domain.WorkOerders.Enums;
using MechanicShop.Domain.WorkOerders.Events;
namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder
{
    public class DeleteWorkOrderCommandHandler(IAppDbContext context , HybridCache cache  ,
        ILogger<DeleteWorkOrderCommandHandler> logger ) 
        : IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
    {
        public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand command, CancellationToken ct)
        {

            var workOrder = await context.WorkOrders
            .FirstOrDefaultAsync(a => a.Id == command.WorkOrderId, ct);

            if (workOrder is null)
            {
                logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }

            if (workOrder.State is not WorkOrderState.Scheduled)
            {
                   logger.LogError(
                    "Deletion failed: only 'Scheduled' or 'Confirmed' WorkOrders can be deleted. Current status: {Status}",
                    workOrder.State);

                return WorkOrderErrors.Readonly;
            }

            context.WorkOrders.Remove(workOrder);

            workOrder.AddDomainEvent(new WorkOrderCollectionModified());

            await context.SaveChangesAsync(ct);

          

            await cache.RemoveByTagAsync("work-order", ct);

            return Result.deleted;
        }
    }
}
