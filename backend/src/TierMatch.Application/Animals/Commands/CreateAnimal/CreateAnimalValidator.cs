using FluentValidation;
using TierMatch.Application.Common.Validation;

namespace TierMatch.Application.Animals.Commands.CreateAnimal;

public class CreateAnimalValidator
    : AbstractValidator<CreateAnimalCommand>
{
    public CreateAnimalValidator()
    {
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