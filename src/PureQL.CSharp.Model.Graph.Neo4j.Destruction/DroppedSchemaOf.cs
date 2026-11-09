using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction;

/// <summary>
/// Drops every stored query graph and the schema of a store: opens a session on
/// <see cref="INeo4jStoredQueries.Driver"/> against
/// <see cref="INeo4jStoredQueries.Database"/>, deletes every <c>:Node</c> in auto-commit
/// batches, then runs each statement of <see cref="SchemaDestructionStatements"/>
/// auto-commit.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <remarks>
/// This is the recommended form: each batch and each drop statement commits on its
/// own, so schema statements never share a transaction with data writes. The
/// operation is not atomic: if it fails midway, the nodes deleted so far stay
/// deleted, and running it again finishes the job. Dropping an empty or schema-less
/// database is a no-op.
/// </remarks>
public sealed record DroppedSchemaOf(INeo4jStoredQueries Store)
{
    private readonly long _batchSize = DroppedSchema.BatchSize;

    /// <summary>
    /// Creates the operation deleting <paramref name="batchSize"/> nodes per statement.
    /// </summary>
    /// <param name="store">The database holding the query graphs.</param>
    /// <param name="batchSize">The largest number of nodes one statement deletes.</param>
    internal DroppedSchemaOf(INeo4jStoredQueries store, long batchSize)
        : this(store)
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
        await using IAsyncSession session = Store.Driver.AsyncSession(builder =>
            builder.WithDatabase(Store.Database).WithDefaultAccessMode(AccessMode.Write)
        );
        await new DroppedSchema(session, _batchSize).ApplyAsync(ct);
    }
}
