using MediatR;

using TierMatch.Application.Common.Results;

namespace TierMatch.Application.ShelterRegistrations.Commands.CreateShelterRegistration;

public class CreateShelterRegistrationCommand
    : IRequest<Result<Guid>>
{
    /*
     * Tierheimdaten
     */
    public string ShelterName { get; set; } =
        string.Empty;

    public string Street { get; set; } =
        string.Empty;

    public string HouseNumber { get; set; } =
        string.Empty;

    public string PostalCode { get; set; } =
        string.Empty;

    public string City { get; set; } =
        string.Empty;

    public string Country { get; set; } =
        "DE";

    public string ShelterPhoneNumber { get; set; } =
        string.Empty;

    public string ShelterEmail { get; set; } =
        string.Empty;

    public string Website { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    /*
     * Ansprechpartner
     */
    public string ContactFirstName { get; set; } =
        string.Empty;

    public string ContactLastName { get; set; } =
        string.Empty;

    public string ContactEmail { get; set; } =
        string.Empty;

    public string ContactPhoneNumber { get; set; } =
        string.Empty;

    public string Message { get; set; } =
        string.Empty;
}