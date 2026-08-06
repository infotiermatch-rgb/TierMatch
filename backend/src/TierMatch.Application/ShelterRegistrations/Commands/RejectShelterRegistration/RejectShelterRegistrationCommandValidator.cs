using FluentValidation;

namespace TierMatch.Application.ShelterRegistrations.Commands.RejectShelterRegistration;

public sealed class RejectShelterRegistrationCommandValidator
    : AbstractValidator<RejectShelterRegistrationCommand>
{
    public RejectShelterRegistrationCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage(
                "Es wurde keine gültige Registrierungs-ID angegeben.");

        RuleFor(command => command.Reason)
            .NotEmpty()
            .WithMessage(
                "Bitte geben Sie einen Ablehnungsgrund an.")
            .MaximumLength(2000)
            .WithMessage(
                "Der Ablehnungsgrund darf höchstens 2000 Zeichen enthalten.");
    }
}