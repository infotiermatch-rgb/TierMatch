namespace TierMatch.Application.Common.Results;

public enum ResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    NoContent,
    Created,
}