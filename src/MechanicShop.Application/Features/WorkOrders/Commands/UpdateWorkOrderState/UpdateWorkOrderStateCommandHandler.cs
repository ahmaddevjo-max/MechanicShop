using FluentValidation;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOerders.Enums;
using MechanicShop.Domain.WorkOerders.Events;
using MechanicShop.Domain.Workorders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState
{
    public class UpdateWorkOrderStateCommandHandler(IAppDbContext context, HybridCache cache,
        ILogger<DeleteWorkOrderCommandHandler> logger, TimeProvider dateTime) 
        : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand command, CancellationToken ct)
        {
            var workOrder = await context.WorkOrders.FirstOrDefaultAsync(w => w.Id == command.WorkOrderId,ct);

            if (workOrder is null)
            {
                logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }


            if(workOrder.StartAtUtc > dateTime.GetUtcNow())
            {
                logger.LogError("State transition for WorkOrder Id '{WorkOrderId}` is not allowed before the work orders scheduled start time.", command.WorkOrderId);

                return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc);
            }




            var updateStatusResult = workOrder.UpdateState(command.State);

            if (updateStatusResult.IsError)
            {
                logger.LogError("Failed to update status: {Error}", updateStatusResult.TopError.Descrption);

                return updateStatusResult.Errors;
            }


            if (command.State == WorkOrderState.Completed)
            {
                workOrder.AddDomainEvent(new OnWorkOrderCompleted { WorkOrderId = command.WorkOrderId });
            }


            workOrder.AddDomainEvent(new WorkOrderCollectionModified());

            await context.SaveChangesAsync(ct);

         

            await cache.RemoveByTagAsync("work-order", ct);

            return Result.updated;


        }
    }
}
