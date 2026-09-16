using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer
{
    public sealed record UpdateCustomerCommand(Guid customerId, string name, string email, string phoneNumber , List<UpdateVehicleCommand> vehicles)
        : IRequest<Result<Updated>>;
    
    
}
