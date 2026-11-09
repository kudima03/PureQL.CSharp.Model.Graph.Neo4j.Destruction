namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

[CollectionDefinition(Name)]
public sealed record SharedNeo4j : ICollectionFixture<Neo4jFixture>
{
    public const string Name = "Neo4j";
}
