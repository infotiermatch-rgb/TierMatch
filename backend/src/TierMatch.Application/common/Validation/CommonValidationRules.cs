using FluentValidation;

namespace TierMatch.Application.Common.Validation;

public static class CommonValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Die E-Mail-Adresse ist erforderlich.")
            .EmailAddress()
            .WithMessage("Bitte geben Sie eine gültige E-Mail-Adresse ein.");
    }

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Die Telefonnummer ist erforderlich.")
            .MaximumLength(30)
            .WithMessage("Die Telefonnummer darf maximal 30 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidWebsite<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(250)
            .WithMessage("Die Website darf maximal 250 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidDescription<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(2000)
            .WithMessage("Die Beschreibung darf maximal 2000 Zeichen lang sein.");
    }
}