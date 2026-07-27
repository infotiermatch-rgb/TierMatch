using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.DeleteAnimal;

public class DeleteAnimalHandler
    : IRequestHandler<DeleteAnimalCommand, bool>
{
    private readonly IAnimalRepository _repository;

    public DeleteAnimalHandler(IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        DeleteAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (animal is null)
            return false;

        _repository.Delete(animal);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}