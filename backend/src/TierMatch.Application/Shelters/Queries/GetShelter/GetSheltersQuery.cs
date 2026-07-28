using MediatR;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelters;

public class GetSheltersQuery : IRequest<List<ShelterDto>>
{
}