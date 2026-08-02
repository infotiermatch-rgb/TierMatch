using FluentValidation;
using TierMatch.Application.Common.Validation;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

public class UpdateAnimalValidator
    : AbstractValidator<UpdateAnimalCommand>
{
    public UpdateAnimalValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Die ID ist erforderlich.");

        RuleFor(x => x.Name)
            .ValidAnimalName();

        RuleFor(x => x.Breed)
            .ValidAnimalBreed();

        RuleFor(x => x.Description)
            .ValidDescription();

        RuleFor(x => x.BirthDate)
            .ValidAnimalBirthDate();

        RuleFor(x => x.ShelterId)
            .ValidAnimalShelterId();
    }
}