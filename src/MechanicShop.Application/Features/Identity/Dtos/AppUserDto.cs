

using MechanicShop.Domain.Identity;
using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.Dtos
{
    public record AppUserDto(string UserId , string Email , List<Role>Rols , List<Claim>claims);
}
