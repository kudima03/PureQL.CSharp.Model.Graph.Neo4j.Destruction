using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

[Collection(SharedNeo4j.Name)]
public sealed record DroppedSchemaTests
{
    private readonly IDriver _driver;

    private readonly SeededDatabase _database;

    public DroppedSchemaTests(Neo4jFixture fixture)
    {
        _driver = fixture.Driver;
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public async Task RemovesEveryQueryNodeOnSession()
    {
        _ = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DroppedSchema(session).ApplyAsync();
        Assert.Equal(0, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public async Task DropsConstraintAndIndexesOnSession()
    {
        _ = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DroppedSchema(session).ApplyAsync();
        Assert.Equal([SeededDatabase.MarkerIndex], await _database.SchemaAsync());
    }

    [Fact]
    public async Task RemovesEveryQueryNodeInBatchesOfOne()
    {
        _ = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DroppedSchema(session, 1).ApplyAsync();
        Assert.Equal(0, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public async Task FailsInTransactionThatDeletedNodes()
    {
        long nodes = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await using IAsyncTransaction transaction = await session.BeginTransactionAsync();
        _ = await Assert.ThrowsAnyAsync<Neo4jException>(() =>
            new DroppedSchema(transaction).ApplyAsync()
        );
        Assert.Equal(nodes, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public async Task KeepsQueriesWhenCancelled()
    {
        long nodes = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DroppedSchema(session).ApplyAsync(new CancellationToken(true))
        );
        Assert.Equal(nodes, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public void DeletesTenThousandNodesPerStatement()
    {
        Assert.Equal(10000, DroppedSchema.BatchSize);
    }

    [Fact]
    public async Task KeepsRunner()
    {
        await using IAsyncSession session = _driver.AsyncSession();
        Assert.Same(session, new DroppedSchema(session).Runner);
    }
}
