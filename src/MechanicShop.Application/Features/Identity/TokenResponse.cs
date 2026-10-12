
namespace MechanicShop.Application.Features.Identity
{
    public class TokenResponse
    {
        public string AccessToken { get; init; } = default!;
        public string RefreshToken { get; init; } = default!;
        public DateTime ExpiresOnUtc {  get; init; }
    }
}
