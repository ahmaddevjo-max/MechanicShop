

using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOerders.Events;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers
{
    public class WorkOrderCollectionModifiedEventHandler(IWorkOrderNotifier notifier) 
        : INotificationHandler<WorkOrderCollectionModified>
    {
        public async Task Handle(WorkOrderCollectionModified notification, CancellationToken ct)
        {
            await notifier.NotifyWorkOrdersChangedAsync(ct);
        }
    }
}
