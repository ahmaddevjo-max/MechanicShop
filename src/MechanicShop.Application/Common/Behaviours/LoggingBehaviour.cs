

using MechanicShop.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace MechanicShop.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
        : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        public async Task Process(TRequest request, CancellationToken ct)
        {
           

            var requestName = typeof(TRequest).Name;
            var userId = user.Id ?? string.Empty;
            string? userName = string.Empty;

            if(!string.IsNullOrEmpty(userId))
            {
                 userName = await identityService.GetUserNameAsync(userId) ;
            }

            logger.LogInformation(
           "Request: {Name} {@UserId} {@UserName} {@Request}", requestName, userId, userName, request);


        }
    }
}
