namespace TierMatch.Application.Authorization;

public static class Policies
{
    public const string Admin = "Admin";

    public const string ShelterAdmin = "ShelterAdmin";

    public const string User = "User";

    public const string CanManageShelter = "CanManageShelter";
}