using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Application.Common.Results;


namespace TierMatch.Application.Animals.Commands.CreateAnimal;

public class CreateAnimalHandler
    : IRequestHandler<CreateAnimalCommand, Result<Guid>>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAnimalHandler(IAnimalRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(animal.Id);
    }
}