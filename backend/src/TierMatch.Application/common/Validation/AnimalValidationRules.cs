using FluentValidation;

namespace TierMatch.Application.Common.Validation;

public static class AnimalValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidAnimalName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Der Name ist erforderlich.")
            .MaximumLength(100)
            .WithMessage("Der Name darf maximal 100 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidAnimalBreed<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Die Rasse ist erforderlich.")
            .MaximumLength(100)
            .WithMessage("Die Rasse darf maximal 100 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, DateOnly?> ValidAnimalBirthDate<T>(
        this IRuleBuilder<T, DateOnly?> ruleBuilder)
    {
        return ruleBuilder
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Das Geburtsdatum darf nicht in der Zukunft liegen.");
    }

    public static IRuleBuilderOptions<T, Guid?> ValidAnimalShelterId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .WithMessage("Ein Tierheim muss ausgewählt werden.");
    }
}