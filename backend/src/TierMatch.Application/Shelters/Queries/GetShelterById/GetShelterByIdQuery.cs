using MediatR;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelterById;

public class GetShelterByIdQuery : IRequest<ShelterDto?>
{
    public Guid Id { get; set; }
}