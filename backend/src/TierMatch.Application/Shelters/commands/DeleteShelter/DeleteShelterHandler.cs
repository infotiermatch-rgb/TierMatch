using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Shelters.Commands.DeleteShelter;

public class DeleteShelterHandler
    : IRequestHandler<DeleteShelterCommand, Result>
{
    private readonly IShelterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteShelterHandler(
        IShelterRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteShelterCommand request,
        CancellationToken cancellationToken)
    {
        var shelter = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (shelter is null)
        {
            return Result.NotFound(
                "Tierheim wurde nicht gefunden.");
        }

        _repository.Delete(shelter);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}