using MediatR;
namespace MechanicShop.Application.Common.Behaviours;

using FluentValidation;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Common.Results.Abstraction;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
       
        if (!validators.Any())
        {
            return await next(ct);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context,ct)));

        var failures = validationResults.SelectMany(v => v.Errors).Where(e => e != null).ToList();

        if (failures.Count == 0)
        {
            return await next(ct);
        }

        var errors = failures.ConvertAll(error => Error.Validation(
            code: error.PropertyName,
            description: error.ErrorMessage));

        return (dynamic)errors;
    }
}