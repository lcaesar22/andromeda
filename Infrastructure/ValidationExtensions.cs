using FluentValidation;

namespace Andromeda.Infrastructure;

public static class ValidationExtensions
{
    public static async Task<IResult?> ValidateAndReturnProblemsAsync<T>(this IValidator<T> validator, T request)
    {
        var result = await validator.ValidateAsync(request);
        return result.IsValid ? null : Results.ValidationProblem(result.ToDictionary());
    }
}