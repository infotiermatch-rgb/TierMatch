using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Shelters.Commands.UpdateShelter;

public class UpdateShelterCommand : IRequest<Result>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string HouseNumber { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = "DE";

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}