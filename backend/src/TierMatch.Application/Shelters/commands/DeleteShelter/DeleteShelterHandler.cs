using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Shelters.Commands.DeleteShelter;

public class DeleteShelterHandler
    : IRequestHandler<DeleteShelterCommand, bool>
{
    private readonly IShelterRepository _repository;

    public DeleteShelterHandler(IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        DeleteShelterCommand request,
        CancellationToken cancellationToken)
    {
        var shelter = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (shelter is null)
        {
            return false;
        }

        _repository.Delete(shelter);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}