

using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens
{
    public class RefreshTokenQueryHandler(ITokenProvider tokenProvider , IAppDbContext context ,
        ILogger<RefreshTokenQueryHandler>logger , IIdentityService identityService)
        : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery query, CancellationToken ct)
        {

            var principal = tokenProvider.GetPrincipalFromExpiredToken(query.ExpiredAccessToken);


            if(principal is null)
            {
                logger.LogError("Expired access token is not valid");
                return ApplicationErrors.ExpiredAccessTokenInvalid;
            }


            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                logger.LogError("Invalid userId claim in expired access token");

                return ApplicationErrors.UserIdClaimInvalid;
            }


            var getUserResult = await identityService.GetUserByIdAsync(userId);


            if (getUserResult.IsError)
            {
                logger.LogError("Get user by id error occurred: {ErrorDescription}", getUserResult.TopError.Descrption);
                return getUserResult.Errors;
            }


            var refreshToken =  await context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token==query.RefreshToken && r.UserId == userId);


            if(refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
            {
                logger.LogError("Refresh token has expired");
                return ApplicationErrors.RefreshTokenExpired;
            }


            var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(getUserResult.Value,ct);


            if (generateTokenResult.IsError)
            {
                logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Descrption);

                return generateTokenResult.Errors;
            }


            return generateTokenResult.Value;


        }
    }
}
