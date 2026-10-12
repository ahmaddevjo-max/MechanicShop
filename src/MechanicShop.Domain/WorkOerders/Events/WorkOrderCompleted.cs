using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.WorkOerders.Events
{
    public sealed class OnWorkOrderCompleted : DomainEvent
    { 
        public Guid WorkOrderId { get; set; }
    }
}
