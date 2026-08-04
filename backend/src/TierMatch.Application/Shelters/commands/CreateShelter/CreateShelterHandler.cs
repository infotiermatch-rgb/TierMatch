using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Shelters.Commands.CreateShelter;

public sealed class CreateShelterHandler
    : IRequestHandler<CreateShelterCommand, Result<Guid>>
{
    private readonly IShelterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateShelterHandler(
        IShelterRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        CreateShelterCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<Guid>.Unauthorized();
        }

        if (!_currentUserService.IsInRole(Roles.Admin))
        {
            return Result<Guid>.Forbidden();
        }

        var shelter = new Shelter
        {
            Name = request.Name,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            PostalCode = request.PostalCode,
            City = request.City,
            Country = request.Country,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Website = request.Website,
            Description = request.Description
        };

        await _repository.AddAsync(
            shelter,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            shelter.Id);
    }
}