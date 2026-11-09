using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction;

/// <summary>
/// Deletes at most <see cref="Size"/> query graph nodes with their relationships and
/// returns how many it deleted, as <c>deleted</c>.
/// </summary>
/// <param name="Size">The largest number of nodes one run deletes.</param>
internal sealed record NodeBatchDeletionStatement(long Size)
{
    /// <summary>The number of nodes deleted per batch by default: 10000.</summary>
    public const long DefaultSize = 10000;

    /// <summary>The name of the batch size parameter.</summary>
    public const string SizeParameter = "batch";

    /// <summary>The name of the returned count of deleted nodes.</summary>
    public const string DeletedColumn = "deleted";

    /// <summary>
    /// <c>MATCH (n:Node) WITH n LIMIT $batch DETACH DELETE n RETURN count(n) AS deleted</c>.
    /// </summary>
    public DriverQuery Value =>
        new DriverQuery(
            $"MATCH (n:{GraphLabels.Node}) WITH n LIMIT ${SizeParameter} "
                + $"DETACH DELETE n RETURN count(n) AS {DeletedColumn}",
            new Dictionary<string, object>() { [SizeParameter] = Size }
        );
}
