using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mappings;

public static class AdoptionRequestMappings
{
    public static AdoptionRequestDto ToDto(
        this AdoptionRequest request)
    {
        return new AdoptionRequestDto
        {
            Id = request.Id,
            AnimalId = request.AnimalId,
            AnimalName = request.Animal.Name,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Message = request.Message,
            Status = request.Status,
            RequestedAt = request.RequestedAt
        };
    }
}