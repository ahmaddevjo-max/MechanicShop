

using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Billing.Queris.GetInvoiceById
{
    public record GetInvoiceByIdQuery(Guid InvoiceId) : ICachedQuery<Result<InvoiceDto>>
    {
        public string CacheKey => $"invoice_{InvoiceId}";

        public string[] Tags => ["invoice"];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
