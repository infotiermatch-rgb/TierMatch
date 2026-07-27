using FluentValidation;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

public class UpdateAnimalValidator : AbstractValidator<UpdateAnimalCommand>
{
    public UpdateAnimalValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Breed)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Species)
            .IsInEnum();

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.Size)
            .IsInEnum();
    }
}