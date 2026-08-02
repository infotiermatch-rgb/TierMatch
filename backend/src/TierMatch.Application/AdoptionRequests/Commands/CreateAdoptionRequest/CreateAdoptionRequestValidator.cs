using FluentValidation;
using TierMatch.Application.Common.Validation;

namespace TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;

public class CreateAdoptionRequestValidator
    : AbstractValidator<CreateAdoptionRequestCommand>
{
    public CreateAdoptionRequestValidator()
    {
        RuleFor(x => x.AnimalId)
            .ValidAdoptionAnimalId();

        RuleFor(x => x.FirstName)
            .ValidAdoptionFirstName();

        RuleFor(x => x.LastName)
            .ValidAdoptionLastName();

        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.PhoneNumber)
            .ValidPhoneNumber();

        RuleFor(x => x.Message)
            .ValidAdoptionMessage();
    }
}