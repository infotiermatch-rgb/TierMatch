using MediatR;

namespace TierMatch.Application.Shelters.Commands.DeleteShelter;

public class DeleteShelterCommand : IRequest<bool>
{
    public DeleteShelterCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}