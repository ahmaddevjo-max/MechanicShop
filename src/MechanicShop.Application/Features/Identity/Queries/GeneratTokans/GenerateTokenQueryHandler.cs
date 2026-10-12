

using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GeneratTokans
{
    public class GenerateTokenQueryHandler(IIdentityService identityService , ITokenProvider tokenProvider ,ILogger<GenerateTokenQuery> logger)
        : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(GenerateTokenQuery query, CancellationToken ct)
        {


            var userResponse = await identityService.AuthenticateAsync(query.Email,query.Password);

            if(userResponse.IsError)
            {
                return userResponse.Errors;
            }

            
            var userInfo = userResponse.Value;
            var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(userInfo, ct);

           
            if (generateTokenResult.IsError)
            {
                logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Descrption);
                return generateTokenResult.Errors;
            }

            return generateTokenResult.Value;

        }
    }
}
