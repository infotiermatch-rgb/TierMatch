using MediatR;

namespace TierMatch.Application.Animals.Commands.DeleteAnimal;

public record DeleteAnimalCommand(Guid Id) : IRequest<bool>;