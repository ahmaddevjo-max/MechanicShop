using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOerders.Billing;
using  MechanicShop.Domain.WorkOerders.Enums;
using MechanicShop.Domain.Workorders;
using MechanicShop.Domain.Workorders.Billing;

namespace MechanicShop.Domain.WorkOrders
{
    public sealed class  WorkOrder : AuditableEntity
    {

        public Guid VehicleId { get; }
        public Vehicle? Vehicle { get; set; }
        public DateTimeOffset StartAtUtc { get; private set; }
        public DateTimeOffset EndAtUtc { get; private set; }
        public Guid LaborId { get; private set; }
        public Employee? Labor { get; set; }
        public Spot Spot { get; private set; }
        public WorkOrderState State { get; private set; }
        public Invoice? Invoice { get; set; }
        public decimal? Discount { get; private set; }
        public decimal? Tax { get; private set; }
        public decimal? TotalPartsCost => _repairTasks.SelectMany(task => task.Parts).Sum(p => p.Cost * p.Quantity);
        public decimal? TotalLaborCost => _repairTasks.Sum(r => r.LaborCost);
        public decimal? Total => (TotalPartsCost ?? 0) + (TotalLaborCost ?? 0);

        private readonly List<RepairTask> _repairTasks = [];
        public IEnumerable<RepairTask> RepairTasks => _repairTasks.AsReadOnly();

        public bool IsEditable => State is not (WorkOrderState.Completed or WorkOrderState.Cancelled or WorkOrderState.InProgress);

        private WorkOrder()
        { }

        private WorkOrder(Guid id ,Guid vehicleId ,Guid laborId , WorkOrderState state , DateTimeOffset startAtUtc , DateTimeOffset endAtUtc ,
            Spot spot , decimal discount, List<RepairTask> repairTasks): base(id)
        {
            VehicleId = vehicleId;
            LaborId = laborId;
            StartAtUtc = startAtUtc;
            EndAtUtc = endAtUtc;
            Spot = spot;
            Discount = discount;
            State = state;
            _repairTasks = repairTasks;
            
        }



        public static Result<WorkOrder> Create(Guid id, Guid vehicleId, Guid laborId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc,
            Spot spot, decimal discount, List<RepairTask> repairTasks)
        {


            if(id == Guid.Empty)
            {
                return WorkOrderErrors.WorkOrderIdRequired;
            }

            if(laborId == Guid.Empty)
            {
                return WorkOrderErrors.LaborIdRequired;
            }


            if (vehicleId == Guid.Empty)
            {
                return WorkOrderErrors.VehicleIdRequired;
            }


            if(repairTasks is null || repairTasks.Count == 0)
            {
                return WorkOrderErrors.RepairTasksRequired;
            }


          if(endAtUtc <= startAtUtc)
            {
                return WorkOrderErrors.InvalidTiming;
            }

            if(!Enum.IsDefined(spot))
            {
                return WorkOrderErrors.SpotInvalid;
            }

            if(discount < 0)
            {
                return WorkOrderErrors.DiscountNegative;
            }


            return new WorkOrder(id,vehicleId,laborId, WorkOrderState.Scheduled,startAtUtc,endAtUtc,spot,discount,repairTasks);

        }



        public Result<Updated> AddRepairTask(RepairTask repairTask)
        {

            if(!IsEditable)
            {
                return WorkOrderErrors.Readonly;
            }


            if(_repairTasks.Any( re => re.Id == repairTask.Id))
            {
                return WorkOrderErrors.RepairTaskAlreadyAdded;
            }


            _repairTasks.Add(repairTask);

            return Result.updated;

        }



        public Result<Updated> UpdateTiming(DateTimeOffset startAtUtc , DateTimeOffset endAtUtc)
        {
           if(!IsEditable)
            {
                return WorkOrderErrors.Readonly;
            }

           
            if(endAtUtc <= startAtUtc)
            {
                return WorkOrderErrors.InvalidTiming ;
            }


            StartAtUtc = startAtUtc;
            EndAtUtc = endAtUtc;

            return Result.updated;
        }


        public Result<Updated> UpdateLabor(Guid laborId)
        {


            if(!IsEditable)
            {
                return WorkOrderErrors.Readonly;
            }


            if (laborId == Guid.Empty)
            {
                return WorkOrderErrors.LaborIdEmpty(laborId.ToString());
            }


            LaborId = laborId;

            return Result.updated;
        }


        public Result<Updated> UpdateState(WorkOrderState newState)
        {
            if(!CanTransitionTo(newState))
            {
                return WorkOrderErrors.InvalidStateTransition(State, newState);
            }

            State = newState;
 
            return Result.updated;

        }

        public bool CanTransitionTo(WorkOrderState newState)
        {
            return (State, newState) switch
            {
                (WorkOrderState.Scheduled, WorkOrderState.InProgress) => true,
                (WorkOrderState.InProgress, WorkOrderState.Completed) =>true,
                (_ , WorkOrderState.Cancelled) when State != WorkOrderState.Completed => true,
                _ => false
            };
        }

        public Result<Updated> Cancel()
        {
            if(!CanTransitionTo(WorkOrderState.Cancelled))
            {
                return WorkOrderErrors.InvalidStateTransition(State , WorkOrderState.Cancelled);
            }

            State = WorkOrderState.Cancelled;

            return Result.updated;
        }


        public Result<Updated> ClearRepairTasks()
        {
            if (!IsEditable)
            {
                return WorkOrderErrors.Readonly;
            }

            _repairTasks.Clear();

            return Result.updated;
        }


        public Result<Updated> UpdateSpot(Spot newSpot)
        {
            if (!IsEditable)
            {
                return WorkOrderErrors.Readonly;
            }

            if (!Enum.IsDefined(newSpot))
            {
                return WorkOrderErrors.SpotInvalid;
            }

            Spot = newSpot;

            return Result.updated;
        }

    }
}
