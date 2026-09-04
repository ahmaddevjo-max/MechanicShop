using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.WorkOerders.Events
{
    public sealed class WorkeOrderCompleted : DomainEvent
    { 
        public Guid WorkOrderId { get; set; }
    }
}
