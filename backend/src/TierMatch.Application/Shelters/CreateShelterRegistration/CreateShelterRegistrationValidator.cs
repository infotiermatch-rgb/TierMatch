using FluentValidation;

using TierMatch.Application.Common.Validation;

namespace TierMatch.Application.ShelterRegistrations.Commands.CreateShelterRegistration;

public sealed class CreateShelterRegistrationValidator
    : AbstractValidator<CreateShelterRegistrationCommand>
{
    public CreateShelterRegistrationValidator()
    {
        /*
         * Tierheimdaten
         */
        RuleFor(x => x.ShelterName)
            .ValidShelterName();

        RuleFor(x => x.Street)
            .ValidShelterStreet();

        RuleFor(x => x.HouseNumber)
            .ValidShelterHouseNumber();

        RuleFor(x => x.PostalCode)
            .ValidShelterPostalCode();

        RuleFor(x => x.City)
            .ValidShelterCity();

        RuleFor(x => x.Country)
            .ValidShelterCountry();

        RuleFor(x => x.ShelterEmail)
            .ValidEmail();

        RuleFor(x => x.ShelterPhoneNumber)
            .ValidPhoneNumber();

        RuleFor(x => x.Website)
            .ValidWebsite();

        RuleFor(x => x.Description)
            .ValidDescription();

        /*
         * Ansprechpartner
         */
        RuleFor(x => x.ContactFirstName)
            .NotEmpty()
            .WithMessage(
                "Der Vorname des Ansprechpartners ist erforderlich.")
            .MaximumLength(100)
            .WithMessage(
                "Der Vorname des Ansprechpartners darf maximal 100 Zeichen lang sein.");

        RuleFor(x => x.ContactLastName)
            .NotEmpty()
            .WithMessage(
                "Der Nachname des Ansprechpartners ist erforderlich.")
            .MaximumLength(100)
            .WithMessage(
                "Der Nachname des Ansprechpartners darf maximal 100 Zeichen lang sein.");

        RuleFor(x => x.ContactEmail)
            .ValidEmail();

        RuleFor(x => x.ContactPhoneNumber)
            .ValidPhoneNumber();

        RuleFor(x => x.Message)
            .MaximumLength(2000)
            .WithMessage(
                "Die Nachricht darf maximal 2000 Zeichen lang sein.");
    }
}