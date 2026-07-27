using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

public class UpdateAnimalHandler
    : IRequestHandler<UpdateAnimalCommand, bool>
{
    private readonly IAnimalRepository _repository;

    public UpdateAnimalHandler(IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        UpdateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (animal is null)
            return false;

        animal.Name = request.Name;
        animal.Species = request.Species;
        animal.Breed = request.Breed;
        animal.Gender = request.Gender;
        animal.Size = request.Size;
        animal.BirthDate = request.BirthDate;
        animal.Description = request.Description;
        animal.IsVaccinated = request.IsVaccinated;
        animal.IsCastrated = request.IsCastrated;

        _repository.Update(animal);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}