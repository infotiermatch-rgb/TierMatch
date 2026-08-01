using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

public class UpdateAnimalHandler
    : IRequestHandler<UpdateAnimalCommand, Result>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnimalHandler(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateAnimalCommand request,
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

        animal.Name = request.Name;
        animal.Species = request.Species;
        animal.Breed = request.Breed;
        animal.Gender = request.Gender;
        animal.Size = request.Size;
        animal.BirthDate = request.BirthDate;
        animal.Description = request.Description;
        animal.IsVaccinated = request.IsVaccinated;
        animal.IsCastrated = request.IsCastrated;
        animal.Status = request.Status;
        animal.ShelterId = request.ShelterId;

        _repository.Update(animal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}