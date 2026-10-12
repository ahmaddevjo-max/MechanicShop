

using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOerders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderState
{
    public class GetWorkOrderStatsQueryHandler(IAppDbContext context) 
             : IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
    {
        public async Task<Result<TodayWorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery query, CancellationToken ct)
        {


            var startDay = query.Date.ToDateTime(TimeOnly.MinValue , DateTimeKind.Utc);
            var endDay = startDay.AddDays(1);


            var WorkOrdersToDayQuery = context.WorkOrders
            .Include(w => w.Vehicle)
            .Include(w => w.RepairTasks).ThenInclude(rt => rt.Parts)
            .Include(w => w.Invoice)
            .Where(w => w.StartAtUtc >= startDay && w.StartAtUtc < endDay);

            var total = await WorkOrdersToDayQuery.CountAsync(ct);

            if(total == 0)
            {
                return new TodayWorkOrderStatsDto
                {
                    Date = query.Date,
                    Total = 0,
                    Scheduled = 0,
                    InProgress = 0,
                    Completed = 0,
                    Cancelled = 0,
                    TotalRevenue = 0,
                    TotalPartsCost = 0,
                    TotalLaborCost = 0,
                    UniqueVehicles = 0,
                    UniqueCustomers = 0
                };
            }


            var WorkOrdersToDay = await WorkOrdersToDayQuery.ToListAsync(ct);

            var totalRevenue = WorkOrdersToDay.Sum(wo => wo.Invoice?.Total ?? 0);

            var totalPartCost = WorkOrdersToDay.Where(wo => wo.Invoice != null).Sum(wo => wo.TotalPartsCost ?? 0);

            var totalLaborCost = WorkOrdersToDay.Where(wo => wo.Invoice != null).Sum(wo => wo.TotalLaborCost ?? 0);
            var uniqueVehicles = WorkOrdersToDay.Select(x => x.VehicleId).Distinct().Count();
            var uniqueCustomers = WorkOrdersToDay.Select(x => x.Vehicle!.CustomerId).Distinct().Count();

            var netProfit = totalRevenue - (totalPartCost + totalLaborCost);


            return new TodayWorkOrderStatsDto
            {
                Date = query.Date,
                Total = total,
                Scheduled = WorkOrdersToDay.Count(x => x.State == WorkOrderState.Scheduled),
                InProgress = WorkOrdersToDay.Count(x => x.State == WorkOrderState.InProgress),
                Completed = WorkOrdersToDay.Count(x => x.State == WorkOrderState.Completed),
                Cancelled = WorkOrdersToDay.Count(x => x.State == WorkOrderState.Cancelled),
                TotalRevenue = totalRevenue,
                TotalPartsCost = totalPartCost,
                TotalLaborCost = totalLaborCost,
                UniqueVehicles = uniqueVehicles,
                UniqueCustomers = uniqueCustomers,
                NetProfit = netProfit,
                ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
                CompletionRate = total > 0 ? ((decimal)WorkOrdersToDay.Count(x => x.State == WorkOrderState.Completed) / total) * 100 : 0,
                AverageRevenuePerOrder = total > 0 ? totalRevenue / total : 0,
                OrdersPerVehicle = uniqueVehicles > 0 ? (decimal)total / uniqueVehicles : 0,
                PartsCostRatio = totalRevenue > 0 ? (totalPartCost / totalRevenue) * 100 : 0,
                LaborCostRatio = totalRevenue > 0 ? (totalLaborCost / totalRevenue) * 100 : 0,
                CancellationRate = total > 0 ? ((decimal)WorkOrdersToDay.Count(x => x.State == WorkOrderState.Cancelled) / total) * 100 : 0
            };



        }

    }


}
