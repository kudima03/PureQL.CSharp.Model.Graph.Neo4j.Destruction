namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

public sealed record NodeBatchDeletionStatementTests
{
    [Fact]
    public void DeletesLimitedNodesWithRelationships()
    {
        Assert.Equal(
            "MATCH (n:Node) WITH n LIMIT $batch DETACH DELETE n "
                + "RETURN count(n) AS deleted",
            new NodeBatchDeletionStatement(5).Value.Text
        );
    }

    [Fact]
    public void PassesSizeAsBatch()
    {
        Assert.Equal(
            new Dictionary<string, object> { ["batch"] = 5L },
            new NodeBatchDeletionStatement(5).Value.Parameters
        );
    }

    [Fact]
    public void DeletesTenThousandByDefault()
    {
        Assert.Equal(10000, NodeBatchDeletionStatement.DefaultSize);
    }

    [Fact]
    public void ReturnsDeletedColumn()
    {
        Assert.Equal("deleted", NodeBatchDeletionStatement.DeletedColumn);
    }
}
