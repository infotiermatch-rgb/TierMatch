using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.DeleteAnimal;

public class DeleteAnimalHandler
    : IRequestHandler<DeleteAnimalCommand, bool>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnimalHandler(IAnimalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}