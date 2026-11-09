using Neo4j.Driver;
using Testcontainers.Neo4j;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

public sealed record Neo4jFixture : IAsyncLifetime
{
    private readonly Neo4jContainer _container = new Neo4jBuilder(
        "neo4j:5-community"
    ).Build();

    private IDriver? _driver;

    public IDriver Driver =>
        _driver ?? throw new InvalidOperationException("Neo4j is not started.");

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _driver = GraphDatabase.Driver(_container.GetConnectionString());
        await _driver.VerifyConnectivityAsync();
    }

    public async Task DisposeAsync()
    {
        if (_driver is not null)
        {
            await _driver.DisposeAsync();
        }
        await _container.DisposeAsync();
    }
}
