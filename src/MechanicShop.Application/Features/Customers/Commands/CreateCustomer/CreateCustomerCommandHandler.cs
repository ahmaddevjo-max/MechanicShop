using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler(
    ILogger<CreateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
    )
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
    {
        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {


            var Email = request.Email.Trim().ToLower();

            var exist = await context.Customers.AnyAsync(C => C.Email!.ToLower() == Email , cancellationToken);

            if (exist)
            {
                logger.LogWarning("Customer creation. Email already exists");

                return CustomerErrors.EmailInvalid;
            }




            List<Vehicle> vehicles = [];


            foreach(var v in request.Vehicles)
            {
                var vehicleResult = Vehicle.Create(Guid.NewGuid() , v.Make ,v.Model , v.Year , v.LicensePlate);

                if(vehicleResult.IsError)
                {                 
                    return vehicleResult.error;
                }

                vehicles.Add(vehicleResult.Value);


            }


            var createCustomerResult = Customer.Create
                (
                Guid.NewGuid()
                , request.Name.Trim()
                , request.PhoneNumber.Trim()
                , request.Email.Trim()
                , vehicles
                );

            if (createCustomerResult.IsError)
                return createCustomerResult.error;


            context.Customers.Add(createCustomerResult.Value);

            await context.SaveChangesAsync(cancellationToken);

            var customer = createCustomerResult.Value;

            logger.LogInformation("Customer created successfully. id :{CustomerId}",customer.Id);

            await cache.RemoveByTagAsync("Customer", cancellationToken);

            return customer.ToDto();
           

        } 
    }
}
