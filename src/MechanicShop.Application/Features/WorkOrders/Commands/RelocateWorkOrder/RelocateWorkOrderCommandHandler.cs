using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOerders.Events;
using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandHandler(
    ILogger<RelocateWorkOrderCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy WorkOrderValidator
    )
    : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand command, CancellationToken ct)
    {

        var workOrder = await context.WorkOrders
            .Include(a => a.RepairTasks)
            .Include(a => a.Labor)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == command.WorkOrderId, ct);

        if (workOrder is null)
        {
            logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }


        var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();

        var endAt = command.NewStartAt.Add(duration);

        var checkSpotAvailabilityResult = await WorkOrderValidator.CheckSpotAvailabilityAsync(
            workOrder.Spot,
            command.NewStartAt,
            endAt,
            excludeWorkOrderId: workOrder.Id,
            ct);

        if (checkSpotAvailabilityResult.IsError)
        {
            logger.LogError("Spot: {Spot} is not available.", workOrder.Spot.ToString());

            return checkSpotAvailabilityResult.Errors;
        }

        if (await WorkOrderValidator.IsLaborOccupied(workOrder.LaborId, command.WorkOrderId, command.NewStartAt, endAt))
        {
            logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);

            return ApplicationErrors.LaborOccupied;
        }

        if (await WorkOrderValidator.IsVehicleAlreadyScheduled(workOrder.VehicleId, command.NewStartAt, endAt, command.WorkOrderId))
        {
            logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", workOrder.VehicleId);

            return ApplicationErrors.VehicleSchedulingConflict;
        }


        var updateTimingResult = workOrder.UpdateTiming(command.NewStartAt, endAt);
        if (updateTimingResult.IsError)
        {
            logger.LogError("Failed to update timing: {Error}", updateTimingResult.TopError.Descrption);

            return updateTimingResult.Errors;
        }

        var updateSpotResult = workOrder.UpdateSpot(command.NewSpot);

        if (updateTimingResult.IsError)
        {
            logger.LogError("Failed to update Spot: {Error}", updateSpotResult.TopError.Descrption);

            return updateTimingResult.Errors;
        }

       workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await context.SaveChangesAsync(ct);

       

        await cache.RemoveByTagAsync("work-order", ct);

        return Result.updated;

    }
}
