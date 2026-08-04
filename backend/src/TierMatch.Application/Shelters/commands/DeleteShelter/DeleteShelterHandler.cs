using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Shelters.Commands.DeleteShelter;

public sealed class DeleteShelterHandler
    : IRequestHandler<DeleteShelterCommand, Result>
{
    private readonly IShelterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteShelterHandler(
        IShelterRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        DeleteShelterCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Unauthorized();
        }

        if (!_currentUserService.IsInRole(Roles.Admin))
        {
            return Result.Forbidden();
        }

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