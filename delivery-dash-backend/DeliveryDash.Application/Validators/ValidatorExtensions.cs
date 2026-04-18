using FluentValidation;
using DeliveryDash.Domain.Exceptions;

namespace DeliveryDash.Application.Validators
{
    public static class ValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> NotEmptyWithException<T>(
            this IRuleBuilder<T, string> ruleBuilder, 
            string fieldName)
        {
            return ruleBuilder
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage($"{fieldName} cannot be empty.")
                .WithErrorCode("EmptyField");
        }
    }
}