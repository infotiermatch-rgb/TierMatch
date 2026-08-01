using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.UpdateAnimalStatus;

public class UpdateAnimalStatusHandler
    : IRequestHandler<UpdateAnimalStatusCommand, Result>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnimalStatusHandler(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateAnimalStatusCommand request,
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

        animal.Status = request.Status;

        _repository.Update(animal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}