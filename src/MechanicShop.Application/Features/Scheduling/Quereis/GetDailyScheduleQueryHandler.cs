

using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailyScheduleQuery;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOerders.Enums;
using MechanicShop.Domain.Workorders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MechanicShop.Application.Features.Scheduling.Quereis
{
    public class GetDailyScheduleQueryHandler(IAppDbContext context, TimeProvider datetime) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
    {
        public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery query, CancellationToken ct)
        {
            var LocalStartDay = query.ScheduleDate.ToDateTime(TimeOnly.MinValue);
            var LocalEndDay = LocalStartDay.AddDays(1);


            var startDayUtc = TimeZoneInfo.ConvertTimeToUtc(LocalStartDay, query.TimeZone);
            var endDayUtc = TimeZoneInfo.ConvertTimeToUtc(LocalEndDay, query.TimeZone);

            var AllWorkOrderToDay = await context.WorkOrders
                .Where(wo => wo.StartAtUtc < endDayUtc && wo.EndAtUtc > startDayUtc
                 &&( query.LaborId == null || wo.LaborId == query.LaborId))
                .Include(wo=> wo.RepairTasks)
                .Include(wo =>wo.Vehicle)
                .Include(wo => wo.Labor)
                     .ToListAsync(ct);

            var now = TimeZoneInfo.ConvertTime(datetime.GetUtcNow(), query.TimeZone);

            var schedule = new ScheduleDto
            {
                OndDate = query.ScheduleDate,
                EndOfDay = LocalEndDay < now,
                spots = []
            };


            foreach (var spot in Enum.GetValues<Spot>())
            {

                var woBySpot = AllWorkOrderToDay
                              .Where(w => w.Spot == spot)
                              .OrderBy(w => w.StartAtUtc) 
                              .ToList();

                var current = LocalStartDay;

                var slots = new List< AvailabilitySlotDto > ();

                while(current < LocalEndDay)
                {
                    
                    var next = current.AddMinutes(15);

                    var startPeriodUtc = TimeZoneInfo.ConvertTimeToUtc(current, query.TimeZone);
                    var endPeriodUtc = TimeZoneInfo.ConvertTimeToUtc(next, query.TimeZone);

                    var woWithInPeriod = woBySpot.FirstOrDefault(wo => wo.StartAtUtc < endPeriodUtc && 
                     wo.EndAtUtc > startPeriodUtc);

                    if(woWithInPeriod != null)
                    {
                        

                        if(!slots.Any(wo => wo.WorkOrderId == woWithInPeriod.Id ))
                        {

                            slots.Add(new AvailabilitySlotDto
                            {
                                WorkOrderId = woWithInPeriod.Id,
                                Spot = spot,
                                StartAt = woWithInPeriod.StartAtUtc,
                                EndAt = woWithInPeriod.EndAtUtc,
                                Vehicle = FormatVehicleInfo(woWithInPeriod.Vehicle!),
                                Labor = woWithInPeriod.Labor!.ToDto(),
                                IsOccupied = true,
                                RepairTasks = [.. woWithInPeriod.RepairTasks.ToList().ConvertAll(rt => rt.ToDto())],
                                WorkOrderLocked = !woWithInPeriod.IsEditable,
                                State = woWithInPeriod.State,
                                IsAvailable = false
                            });
                        }


                    }
                    else
                    {

                        slots.Add(new AvailabilitySlotDto
                        {
                            Spot = spot,
                            StartAt = startPeriodUtc,
                            EndAt = endPeriodUtc,
                            WorkOrderLocked = false,
                            IsAvailable = current >= now
                        });


                    }


                    current = next;

                }

                schedule.spots.Add(new SpotDto
                {
                    Spot = spot,
                    Slots = slots,
                });

                
            }

            return schedule;
        }

        private static string? FormatVehicleInfo(Vehicle vehicle) =>
       vehicle != null ? $"{vehicle.Make} | {vehicle.LicensePlate}" : null;
    }
}
