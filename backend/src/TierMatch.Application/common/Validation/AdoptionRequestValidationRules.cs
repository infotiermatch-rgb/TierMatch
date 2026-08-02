using FluentValidation;

namespace TierMatch.Application.Common.Validation;

public static class AdoptionRequestValidationRules
{
    public static IRuleBuilderOptions<T, Guid> ValidAdoptionAnimalId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Es muss ein Tier ausgewählt werden.");
    }

    public static IRuleBuilderOptions<T, string> ValidAdoptionFirstName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Der Vorname ist erforderlich.")
            .MaximumLength(100)
            .WithMessage("Der Vorname darf maximal 100 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidAdoptionLastName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Der Nachname ist erforderlich.")
            .MaximumLength(100)
            .WithMessage("Der Nachname darf maximal 100 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidAdoptionMessage<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(2000)
            .WithMessage("Die Nachricht darf maximal 2000 Zeichen lang sein.");
    }
}