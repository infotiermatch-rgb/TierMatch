using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.DeleteAnimal;

public class DeleteAnimalHandler
    : IRequestHandler<DeleteAnimalCommand, Result>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnimalHandler(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (animal is null)
        {
            return Result.NotFound(
                "Tier wurde nicht gefunden.");
        }

        _repository.Delete(animal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}