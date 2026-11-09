namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction;

/// <summary>
/// The names of the constraint and the indexes that
/// <c>PureQL.CSharp.Model.Graph.Neo4j.Creation</c> creates and this package drops.
/// </summary>
/// <remarks>
/// This package does not reference the Creation package, so the names are duplicated
/// here: they must match the names Creation gives its constraint and indexes, or the
/// drop statements silently leave them in place.
/// </remarks>
public static class SchemaNames
{
    /// <summary>
    /// The unique constraint on <c>Query.id</c>: <c>pureql_query_id</c>.
    /// </summary>
    public const string QueryId = "pureql_query_id";

    /// <summary>
    /// The range index on <c>Node.queryId</c>: <c>pureql_node_query_id</c>.
    /// </summary>
    public const string NodeQueryId = "pureql_node_query_id";

    /// <summary>
    /// The composite range index on <c>Node.field</c> and <c>Node.source</c>:
    /// <c>pureql_node_field_source</c>.
    /// </summary>
    public const string NodeFieldSource = "pureql_node_field_source";

    /// <summary>
    /// The range index on <c>Node.param</c>: <c>pureql_node_param</c>.
    /// </summary>
    public const string NodeParam = "pureql_node_param";
}
