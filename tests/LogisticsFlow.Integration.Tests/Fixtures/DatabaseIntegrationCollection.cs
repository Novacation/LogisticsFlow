namespace LogisticsFlow.Integration.Tests.Fixtures;

[CollectionDefinition(nameof(DatabaseIntegrationCollection))]
public sealed class DatabaseIntegrationCollection
    : ICollectionFixture<MsSqlContainerFixture>;