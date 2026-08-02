using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mapping;

public static class AdoptionRequestMappings
{
    public static AdoptionRequestDto ToDto(this AdoptionRequest request)
    {
        return new AdoptionRequestDto
        {
            Id = request.Id,
            AnimalId = request.AnimalId,
            AnimalName = request.Animal?.Name ?? string.Empty,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Message = request.Message,
            Status = request.Status,
            RequestedAt = request.RequestedAt
        };
    }

    public static List<AdoptionRequestDto> ToDto(
        this IEnumerable<AdoptionRequest> requests)
    {
        return requests
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => x.ToDto())
            .ToList();
    }
}