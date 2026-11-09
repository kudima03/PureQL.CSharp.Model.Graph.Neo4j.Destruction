using System.Collections;
using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction;

/// <summary>
/// The statements dropping the constraint and the indexes of the PureQL query graphs:
/// <c>DROP CONSTRAINT pureql_query_id IF EXISTS</c>, then
/// <c>DROP INDEX &lt;name&gt; IF EXISTS</c> for <c>pureql_node_query_id</c>,
/// <c>pureql_node_field_source</c> and <c>pureql_node_param</c>.
/// </summary>
/// <remarks>
/// Each statement is idempotent: dropping what does not exist is a no-op. The
/// statements change the schema only, so in Neo4j they cannot share an explicit
/// transaction with data writes; run each one auto-commit on a session. Dropping the
/// constraint also drops the index backing it.
/// <para>
/// <see cref="DriverQuery"/> is <c>Neo4j.Driver.Query</c>; in code under the
/// <c>PureQL.CSharp.Model</c> namespace, <c>Query</c> alone names the model's subquery
/// union, so alias the driver type.
/// </para>
/// </remarks>
public sealed record SchemaDestructionStatements : IEnumerable<DriverQuery>
{
    /// <summary>Returns the drop statements, the constraint first.</summary>
    /// <returns>The four drop statements, without parameters.</returns>
    public IEnumerator<DriverQuery> GetEnumerator()
    {
        yield return new DriverQuery($"DROP CONSTRAINT {SchemaNames.QueryId} IF EXISTS");
        yield return new DriverQuery($"DROP INDEX {SchemaNames.NodeQueryId} IF EXISTS");
        yield return new DriverQuery(
            $"DROP INDEX {SchemaNames.NodeFieldSource} IF EXISTS"
        );
        yield return new DriverQuery($"DROP INDEX {SchemaNames.NodeParam} IF EXISTS");
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
