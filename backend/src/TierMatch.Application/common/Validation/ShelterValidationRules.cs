using FluentValidation;

namespace TierMatch.Application.Common.Validation;

public static class ShelterValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidShelterName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Der Name des Tierheims ist erforderlich.")
            .MaximumLength(150)
            .WithMessage("Der Name darf maximal 150 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidShelterStreet<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Die Straße ist erforderlich.")
            .MaximumLength(150)
            .WithMessage("Die Straße darf maximal 150 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidShelterHouseNumber<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Die Hausnummer ist erforderlich.")
            .MaximumLength(20)
            .WithMessage("Die Hausnummer darf maximal 20 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidShelterPostalCode<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Die Postleitzahl ist erforderlich.")
            .Length(5)
            .WithMessage("Die Postleitzahl muss genau 5 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidShelterCity<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Die Stadt ist erforderlich.")
            .MaximumLength(100)
            .WithMessage("Die Stadt darf maximal 100 Zeichen lang sein.");
    }

    public static IRuleBuilderOptions<T, string> ValidShelterCountry<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Das Land ist erforderlich.")
            .Length(2)
            .WithMessage("Der Ländercode muss genau 2 Zeichen lang sein.");
    }
}