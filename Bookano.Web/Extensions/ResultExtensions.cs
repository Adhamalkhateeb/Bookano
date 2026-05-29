using Bookano.Application.Common.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bookano.Web.Extensions;

public static class ResultExtensions
{
    public static void AddToModelState(this Result result, ModelStateDictionary modelState)
    {
        if (result.IsSuccess) return;

        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.Error);
        }

        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            modelState.AddModelError(string.Empty, result.ErrorMessage);
        }
    }
}
