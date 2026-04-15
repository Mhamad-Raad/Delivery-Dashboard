using FluentValidation;

namespace MalDash.Application.Extensions
{
    public static class ValidationExtensions
    {
        public static async Task ValidateAndThrowCustomAsync<T>(
            this IValidator<T> validator,
            T instance)
        {
            var validationResult = await validator.ValidateAsync(instance);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                throw new Domain.Exceptions.ValidationException(errors);
            }
        }
    }
}