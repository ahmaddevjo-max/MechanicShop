
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOerders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers
{
   
    
    public class SendWorkOrderCompletedEmailHandler(INotificationService notificationService,
        IAppDbContext context ,
        ILogger<SendWorkOrderCompletedEmailHandler>logger ) 
       
        : INotificationHandler<OnWorkOrderCompleted>
    
    
    {
        public async Task Handle(OnWorkOrderCompleted notification, CancellationToken ct)
        {
            var workOrder = await context.WorkOrders
                        .Include(w => w.Vehicle!).ThenInclude(v => v.Customer)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.Id == notification.WorkOrderId, ct);

            if (workOrder is null)
            {
                logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", notification.WorkOrderId);
                return;
            }

            await notificationService.SendEmailAsync(workOrder.Vehicle?.Customer?.Email! , ct );
            await notificationService.SendSmSAsync(workOrder.Vehicle?.Customer?.PhoneNumber!, ct);
        }
    }
}
