using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Animals.Commands.CreateAnimal;

public class CreateAnimalHandler
    : IRequestHandler<CreateAnimalCommand, Guid>
{
    private readonly IAnimalRepository _repository;

    public CreateAnimalHandler(IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = new Animal
{
    Name = request.Name,
    Species = request.Species,
    Breed = request.Breed,
    Gender = request.Gender,
    Size = request.Size,
    BirthDate = request.BirthDate,
    Description = request.Description,
    IsVaccinated = request.IsVaccinated,
    IsCastrated = request.IsCastrated,

    ShelterId = request.ShelterId,
    Status = request.Status,
};

        await _repository.AddAsync(animal, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return animal.Id;
    }
}