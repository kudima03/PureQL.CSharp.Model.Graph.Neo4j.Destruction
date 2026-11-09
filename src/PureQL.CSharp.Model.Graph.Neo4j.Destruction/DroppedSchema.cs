using Neo4j.Driver;
using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction;

/// <summary>
/// Drops every stored query graph and the schema on a query runner: deletes every
/// <c>:Node</c> in batches, then runs <see cref="SchemaDestructionStatements"/>.
/// </summary>
/// <param name="Runner">
/// The runner the statements run on: an <see cref="IAsyncSession"/>, where each
/// statement runs in its own auto-commit transaction, or an
/// <see cref="IAsyncTransaction"/>.
/// </param>
/// <remarks>
/// <para>
/// The nodes are deleted <see cref="BatchSize"/> at a time, repeating
/// <c>MATCH (n:Node) WITH n LIMIT $batch DETACH DELETE n RETURN count(n) AS deleted</c>
/// until it deletes none, so that no single transaction grows with the number of stored
/// queries on a session. (<c>CALL { } IN TRANSACTIONS</c> would do the same in one
/// statement, but it cannot run inside an explicit transaction.)
/// </para>
/// <para>
/// In Neo4j, schema statements and data writes cannot share one explicit transaction:
/// when <paramref name="Runner"/> is an <see cref="IAsyncTransaction"/> that deleted
/// nodes, the drop statements fail. Run it on a session, or use
/// <see cref="DroppedSchemaOf"/>, which opens one.
/// </para>
/// <para>
/// Dropping an empty or schema-less database is a no-op.
/// </para>
/// </remarks>
public sealed record DroppedSchema(IAsyncQueryRunner Runner)
{
    /// <summary>The number of nodes deleted per statement: 10000.</summary>
    public const long BatchSize = NodeBatchDeletionStatement.DefaultSize;

    private readonly long _batchSize = BatchSize;

    /// <summary>
    /// Creates the operation deleting <paramref name="batchSize"/> nodes per statement.
    /// </summary>
    /// <param name="runner">The runner the statements run on.</param>
    /// <param name="batchSize">The largest number of nodes one statement deletes.</param>
    internal DroppedSchema(IAsyncQueryRunner runner, long batchSize)
        : this(runner)
    {
        _batchSize = batchSize;
    }

    /// <summary>
    /// Deletes every stored query graph, then drops the constraint and the indexes.
    /// </summary>
    /// <param name="ct">
    /// Cancels the operation between statements and while reading a batch's count.
    /// </param>
    /// <returns>A task completing once the schema is dropped.</returns>
    public async Task ApplyAsync(CancellationToken ct = default)
    {
        DriverQuery deletion = new NodeBatchDeletionStatement(_batchSize).Value;
        long deleted;
        do
        {
            ct.ThrowIfCancellationRequested();
            IResultCursor cursor = await Runner.RunAsync(deletion);
            IRecord record = await cursor.SingleAsync(ct);
            deleted = Convert.ToInt64(
                record[NodeBatchDeletionStatement.DeletedColumn],
                System.Globalization.CultureInfo.InvariantCulture
            );
        } while (deleted > 0);
        foreach (DriverQuery statement in new SchemaDestructionStatements())
        {
            ct.ThrowIfCancellationRequested();
            IResultCursor cursor = await Runner.RunAsync(statement);
            _ = await cursor.ConsumeAsync();
        }
    }
}
