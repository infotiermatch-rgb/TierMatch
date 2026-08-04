using FluentValidation;

using TierMatch.Application.Common.Validation;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

public sealed class UpdateAnimalValidator
    : AbstractValidator<UpdateAnimalCommand>
{
    public UpdateAnimalValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Die Tier-ID ist erforderlich.");

        RuleFor(command => command.Name)
            .ValidAnimalName();

        RuleFor(command => command.Breed)
            .ValidAnimalBreed();

        RuleFor(command => command.Description)
            .ValidDescription();

        RuleFor(command => command.BirthDate)
            .ValidAnimalBirthDate();

        RuleFor(command => command.ShelterId)
            .ValidAnimalShelterId();

        RuleFor(command => command.Species)
            .IsInEnum()
            .WithMessage("Die Tierart ist ungültig.");

        RuleFor(command => command.Gender)
            .IsInEnum()
            .WithMessage("Das Geschlecht ist ungültig.");

        RuleFor(command => command.Size)
            .IsInEnum()
            .WithMessage("Die Größe ist ungültig.");

        RuleFor(command => command.Status)
            .IsInEnum()
            .WithMessage("Der Tierstatus ist ungültig.");
    }
}