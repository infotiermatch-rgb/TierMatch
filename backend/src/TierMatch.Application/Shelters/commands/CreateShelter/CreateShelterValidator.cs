using FluentValidation;

namespace TierMatch.Application.Shelters.Commands.CreateShelter;

public class CreateShelterValidator : AbstractValidator<CreateShelterCommand>
{
    public CreateShelterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.HouseNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Website)
            .Must(uri =>
                string.IsNullOrWhiteSpace(uri) ||
                Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Website must be a valid URL.");
    }
}