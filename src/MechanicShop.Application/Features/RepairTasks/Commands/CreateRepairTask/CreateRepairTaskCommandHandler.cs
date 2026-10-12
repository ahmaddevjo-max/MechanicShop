using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask
{
    public class CreateRepairTaskCommandHandler(IAppDbContext context,
        ILogger <CreateRepairTaskCommandHandler>logger , 
        HybridCache cache) : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
    {
        public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand command, CancellationToken ct)
        {

            var isNameExist = await context.RepairTasks
                .AnyAsync(r => r.Name == command.Name , ct);
            
            if(isNameExist)
            {
                logger.LogWarning("Duplicate part name '{PartName}'.", command.Name);

                return RepairTaskErrors.DuplicateName;
            }

            List<Part> parts = [];

            foreach (var part in command.Parts)
            {
                var newPart = Part.Create(Guid.NewGuid() , part.Name , part.Cost , part.Quantity);

                if(newPart.IsError)
                {
                    return newPart.Errors;
                }

                parts.Add(newPart.Value);

            }



            var createRepairTaskResult = RepairTask.Create(
                        id: Guid.NewGuid(),
                        name: command.Name!,
                        laborCost: command.LaborCost,
                        estimatedDurationInMins: command.EstimatedDurationInMins!.Value,
                        parts: parts);


            if(createRepairTaskResult.IsError)
            {
                return createRepairTaskResult.Errors;
            }


            var repairTask = createRepairTaskResult.Value;
            
            context.RepairTasks.Add(repairTask);

            await context.SaveChangesAsync(ct);

            await cache.RemoveByTagAsync("repair-task", ct);

            return repairTask.ToDto();




        }
    }
}
