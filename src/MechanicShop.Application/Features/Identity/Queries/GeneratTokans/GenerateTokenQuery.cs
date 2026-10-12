
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Queries.GeneratTokans
{
    public record GenerateTokenQuery(string Email , string Password) : IRequest<Result<TokenResponse>>;
    
}
