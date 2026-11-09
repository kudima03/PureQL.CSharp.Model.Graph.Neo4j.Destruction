using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

[Collection(SharedNeo4j.Name)]
public sealed record DroppedSchemaOfTests
{
    private readonly INeo4jStoredQueries _store;

    private readonly SeededDatabase _database;

    public DroppedSchemaOfTests(Neo4jFixture fixture)
    {
        _store = new Neo4jStoredQueries(fixture.Driver);
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public async Task StartsWithSchemaAndQueries()
    {
        long nodes = await _database.SeedAsync();
        Assert.Equal(nodes, await _database.QueryNodeCountAsync());
        Assert.Equal(
            [
                SeededDatabase.MarkerIndex,
                SchemaNames.NodeFieldSource,
                SchemaNames.NodeParam,
                SchemaNames.NodeQueryId,
                SchemaNames.QueryId,
            ],
            await _database.SchemaAsync()
        );
    }

    [Fact]
    public async Task RemovesEveryQueryNode()
    {
        _ = await _database.SeedAsync();
        await new DroppedSchemaOf(_store).ApplyAsync();
        Assert.Equal(0, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public async Task RemovesEveryRelationship()
    {
        _ = await _database.SeedAsync();
        await new DroppedSchemaOf(_store).ApplyAsync();
        Assert.Equal(0, await _database.RelationshipCountAsync());
    }

    [Fact]
    public async Task KeepsNodesOutsideQueryGraphs()
    {
        _ = await _database.SeedAsync();
        await new DroppedSchemaOf(_store).ApplyAsync();
        Assert.Equal(1, await _database.MarkerCountAsync());
    }

    [Fact]
    public async Task DropsOnlyConstraintAndIndexes()
    {
        _ = await _database.SeedAsync();
        await new DroppedSchemaOf(_store).ApplyAsync();
        Assert.Equal([SeededDatabase.MarkerIndex], await _database.SchemaAsync());
    }

    [Fact]
    public async Task IsNoOpWhenAppliedTwice()
    {
        _ = await _database.SeedAsync();
        await new DroppedSchemaOf(_store).ApplyAsync();
        await new DroppedSchemaOf(_store).ApplyAsync();
        Assert.Equal(0, await _database.QueryNodeCountAsync());
        Assert.Equal(1, await _database.MarkerCountAsync());
        Assert.Equal([SeededDatabase.MarkerIndex], await _database.SchemaAsync());
    }

    [Fact]
    public async Task RemovesEveryQueryNodeInSmallBatches()
    {
        long nodes = await _database.SeedAsync();
        Assert.True(nodes > 7);
        await new DroppedSchemaOf(_store, 7).ApplyAsync();
        Assert.Equal(0, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public async Task IsNoOpOnEmptyDatabase()
    {
        await _database.EmptyAsync();
        await new DroppedSchemaOf(_store).ApplyAsync();
        Assert.Equal(0, await _database.QueryNodeCountAsync());
        Assert.Empty(await _database.SchemaAsync());
    }

    [Fact]
    public async Task UsesStoreDatabase()
    {
        long nodes = await _database.SeedAsync();
        _ = await Assert.ThrowsAnyAsync<Neo4jException>(() =>
            new DroppedSchemaOf(
                new Neo4jStoredQueries(_store.Driver, "missing")
            ).ApplyAsync()
        );
        Assert.Equal(nodes, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public async Task KeepsQueriesWhenCancelled()
    {
        long nodes = await _database.SeedAsync();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DroppedSchemaOf(_store).ApplyAsync(new CancellationToken(true))
        );
        Assert.Equal(nodes, await _database.QueryNodeCountAsync());
    }

    [Fact]
    public void KeepsStore()
    {
        Assert.Same(_store, new DroppedSchemaOf(_store).Store);
    }
}
