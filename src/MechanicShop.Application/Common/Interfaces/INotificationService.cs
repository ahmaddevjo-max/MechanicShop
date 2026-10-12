
namespace MechanicShop.Application.Common.Interfaces
{
    public interface INotificationService
    {

        Task SendEmailAsync(string to, CancellationToken ct = default);
        Task SendSmSAsync(string phoneNumber , CancellationToken ct = default);

    }
}
