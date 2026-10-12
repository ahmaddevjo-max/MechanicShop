

using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces
{
    public interface IIdentityService
    {
       Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);
        Task<Result<AppUserDto>> GetUserByIdAsync(string UserId);

        Task<string?> GetUserNameAsync(string userId);
    }
}
