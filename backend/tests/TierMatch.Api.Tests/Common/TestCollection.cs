using Xunit;

namespace TierMatch.Api.Tests.Common;

[CollectionDefinition(Name)]
public sealed class TestCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
    public const string Name = "IntegrationTests";
}