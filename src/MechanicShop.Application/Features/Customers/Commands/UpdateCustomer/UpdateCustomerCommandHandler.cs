using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler(IAppDbContext context ,
        ILogger<UpdateCustomerCommandHandler>logger ,
        HybridCache cache): IRequestHandler< UpdateCustomerCommand, Result<Updated>>
    
    {
       
        public async Task<Result<Updated>> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
        {

            var customer = context.Customers.Include(c => c.vehicles).
                FirstOrDefault(c => c.Id == command.customerId);

            if(customer == null)
            {

                logger.LogWarning("Customer {CustomerId} not found for update.", command.customerId);

                return ApplicationErrors.CustomerNotFound;
            }


            var validatedVehicles = new List<Vehicle>();

            foreach(var v in command.vehicles)
            {
                var vehicleId = v.VehicleId ?? new Guid();

                var vehicleResult = Vehicle.Create(vehicleId , v.Make ,v.Model ,v.Year ,v.LicensePlate);

                if(vehicleResult.IsError)
                {
                    return vehicleResult.error;
                }


                validatedVehicles.Add(vehicleResult.Value);

            }




          var updateCustomerResult = customer.Update(command.name , command.email , command.phoneNumber);

            if (updateCustomerResult.IsError)
            {
                return updateCustomerResult.error;
            }



            var upsertPartsResult = customer.UpsertVehicles(validatedVehicles);

            if(updateCustomerResult.IsError)
            {
                return updateCustomerResult.error;
            }


            await context.SaveChangesAsync(cancellationToken);

            await cache.RemoveByTagAsync("customer", cancellationToken);

            return Result.updated;

        }
    }
}
