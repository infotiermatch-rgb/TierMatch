using FluentValidation;

namespace TierMatch.Application.Animals.Commands.CreateAnimal;

public class CreateAnimalValidator : AbstractValidator<CreateAnimalCommand>
{
    public CreateAnimalValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Der Name ist erforderlich.")
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