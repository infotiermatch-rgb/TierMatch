using FluentValidation;

using TierMatch.Application.Common.Validation;

namespace TierMatch.Application.Animals.Commands.CreateAnimal;

public sealed class CreateAnimalValidator
    : AbstractValidator<CreateAnimalCommand>
{
    public CreateAnimalValidator()
    {
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