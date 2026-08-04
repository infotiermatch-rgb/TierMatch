using Xunit;

namespace TierMatch.Api.Tests.Common;

[CollectionDefinition(
    Name,
    DisableParallelization = true)]
public sealed class TestCollection
    : ICollectionFixture<PostgreSqlContainerFixture>
{
    public const string Name = "IntegrationTests";
}