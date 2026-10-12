using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOerders.Enums;

public interface IWorkOrderPolicy
{

    bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration);
    Task<bool> IsLaborOccupied(Guid laborId, Guid excludedWorkOrderId, DateTimeOffset startAt, DateTimeOffset endAt);

    Result<Success> ValidateMinimumRequirement(DateTimeOffset StartAt , DateTimeOffset EndAt);

    Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null);

    Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot , DateTimeOffset startAt , DateTimeOffset endat  ,Guid? excludeWorkOrderId= null , CancellationToken ct = default);

}