using FluentValidation;
using TierMatch.Application.Common.Validation;

namespace TierMatch.Application.Shelters.Commands.UpdateShelter;

public class UpdateShelterValidator
    : AbstractValidator<UpdateShelterCommand>
{
    public UpdateShelterValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Die ID ist erforderlich.");

        RuleFor(x => x.Name)
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

        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.PhoneNumber)
            .ValidPhoneNumber();

        RuleFor(x => x.Website)
            .ValidWebsite();

        RuleFor(x => x.Description)
            .ValidDescription();
    }
}