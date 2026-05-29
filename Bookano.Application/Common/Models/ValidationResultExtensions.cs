using FluentValidation.Results;
namespace Bookano.Application.Common.Models;

public static class ValidationResultExtensions
{
    public static IEnumerable<ValidationError> ToValidationErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage));
    }
}
