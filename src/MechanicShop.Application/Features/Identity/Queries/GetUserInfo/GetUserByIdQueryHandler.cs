
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo
{
    public class GetUserByIdQueryHandler(IIdentityService identityService,ILogger<GetUserByIdQueryHandler> logger)
        : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
    {
        public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery query, CancellationToken ct)
        {
            var getUserResult =await identityService.GetUserByIdAsync(query.UserId!);

            if(getUserResult.IsError)
            {
                logger.LogError("User with Id { UserId }{ErrorDetails}", query.UserId, getUserResult.TopError.Descrption);
                return getUserResult.Errors;
            }

            return getUserResult.Value;


        }
    }
}
