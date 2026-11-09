using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

/// <summary>
/// Writes a query graph in one statement, as the Insertion package does; duplicated
/// here because the tests may not depend on another operation package.
/// </summary>
internal sealed record InsertedGraph(IAsyncQueryRunner Runner, QueryGraph Graph)
{
    private const string Text =
        "UNWIND $nodes AS n CREATE (x:$(n.labels)) SET x = n.props "
        + "WITH collect(x) AS xs UNWIND $rels AS r "
        + "WITH xs[r.f] AS a, xs[r.t] AS b, r "
        + "CREATE (a)-[e:$(r.type)]->(b) SET e.position = r.position";

    public async Task ApplyAsync()
    {
        IResultCursor cursor = await Runner.RunAsync(
            Text,
            new Dictionary<string, object>
            {
                ["nodes"] = Graph
                    .Nodes.Select(node => new Dictionary<string, object>
                    {
                        ["labels"] = node.Labels.ToArray(),
                        ["props"] = new Dictionary<string, object>(node.Properties),
                    })
                    .ToList(),
                ["rels"] = Graph
                    .Relationships.Select(relationship => new Dictionary<string, object?>
                    {
                        ["f"] = relationship.From,
                        ["t"] = relationship.To,
                        ["type"] = relationship.Type,
                        ["position"] = relationship.Position,
                    })
                    .ToList(),
            }
        );
        _ = await cursor.ConsumeAsync();
    }
}
