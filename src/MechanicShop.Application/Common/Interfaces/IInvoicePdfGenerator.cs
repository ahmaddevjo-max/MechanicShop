
using MechanicShop.Domain.WorkOerders.Billing;

namespace MechanicShop.Application.Common.Interfaces
{
    public interface IInvoicePdfGenerator
    {
        Byte[] Generate(Invoice invoice);
    }
}
