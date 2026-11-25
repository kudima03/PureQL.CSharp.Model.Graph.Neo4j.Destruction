using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

/// <summary>
/// Every sample query stored at once, then the schema dropped: nothing of any graph is
/// left, whatever labels and relationship types its shape gives it.
/// </summary>
[Collection(SharedNeo4j.Name)]
public sealed record EverySampleTests
{
    private readonly INeo4jStoredQueries _store;

    private readonly SeededDatabase _database;

    public EverySampleTests(Neo4jFixture fixture)
    {
        _store = new Neo4jStoredQueries(fixture.Driver);
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public void EverySampleIsLoaded()
    {
        Assert.Equal(386, QuerySamples.All.Count);
    }

    [Fact]
    public async Task RemovesEveryNodeAndRelationshipOfEverySample()
    {
        long nodes = await _database.SeedAsync(QuerySamples.All.Values);
        Assert.True(nodes > DroppedSchema.BatchSize);
        Assert.Equal(nodes, await _database.QueryNodeCountAsync());
        Assert.True(await _database.RelationshipCountAsync() > 0);

        await new DroppedSchemaOf(_store).ApplyAsync();

        Assert.Equal(1, await _database.NodeCountAsync());
        Assert.Equal(1, await _database.MarkerCountAsync());
        Assert.Equal(0, await _database.RelationshipCountAsync());
        Assert.Equal([SeededDatabase.MarkerIndex], await _database.SchemaAsync());
    }
}
