namespace Examples.Idempotency.EndToEnd;

/// <summary>The scenarios that share one <see cref="PostgresFixture"/> container.</summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Payments PostgreSQL";
}
