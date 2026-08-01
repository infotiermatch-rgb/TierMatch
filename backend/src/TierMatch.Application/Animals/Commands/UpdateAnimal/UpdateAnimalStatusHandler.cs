using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.UpdateAnimalStatus;

public class UpdateAnimalStatusHandler
    : IRequestHandler<UpdateAnimalStatusCommand, bool>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnimalStatusHandler(IAnimalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateAnimalStatusCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (animal is null)
            return false;

        animal.Status = request.Status;

        _repository.Update(animal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}