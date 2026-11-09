namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

public sealed record SchemaNamesTests
{
    [Fact]
    public void NamesQueryIdConstraint()
    {
        Assert.Equal("pureql_query_id", SchemaNames.QueryId);
    }

    [Fact]
    public void NamesNodeQueryIdIndex()
    {
        Assert.Equal("pureql_node_query_id", SchemaNames.NodeQueryId);
    }

    [Fact]
    public void NamesNodeFieldSourceIndex()
    {
        Assert.Equal("pureql_node_field_source", SchemaNames.NodeFieldSource);
    }

    [Fact]
    public void NamesNodeParamIndex()
    {
        Assert.Equal("pureql_node_param", SchemaNames.NodeParam);
    }
}
