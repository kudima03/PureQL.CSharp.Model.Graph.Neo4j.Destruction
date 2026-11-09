using Neo4j.Driver;
using PureQL.CSharp.Model.Samples.Queries.GroupBy;
using PureQL.CSharp.Model.Samples.Queries.Joins;
using PureQL.CSharp.Model.Samples.Queries.Pagination;
using PureQL.CSharp.Model.Samples.Queries.Select;
using PureQL.CSharp.Model.Samples.Queries.Subqueries;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

/// <summary>
/// The test database with the schema Creation creates and sample query graphs, next
/// to a node and an index that belong to no query graph.
/// </summary>
internal sealed record SeededDatabase(IDriver Driver)
{
    public const string MarkerLabel = "PureQLModelVersion";

    public const string MarkerIndex = "marker_value";

    private static IReadOnlyList<string> SchemaStatements =>
        [
            "CREATE CONSTRAINT pureql_query_id IF NOT EXISTS "
                + "FOR (q:Query) REQUIRE q.id IS UNIQUE",
            "CREATE INDEX pureql_node_query_id IF NOT EXISTS "
                + "FOR (n:Node) ON (n.queryId)",
            "CREATE INDEX pureql_node_field_source IF NOT EXISTS "
                + "FOR (n:Node) ON (n.field, n.source)",
            "CREATE INDEX pureql_node_param IF NOT EXISTS FOR (n:Node) ON (n.param)",
            $"CREATE INDEX {MarkerIndex} IF NOT EXISTS "
                + $"FOR (m:{MarkerLabel}) ON (m.value)",
        ];

    private static IReadOnlyList<PureQLQuery> Queries =>
        [
            new InnerJoinQuery().Value,
            new FilterOrderAndPageQuery().Value,
            new SubqueryPipelineQuery().Value,
            new LiteralOfEveryTypeNextToFieldQuery().Value,
            new GroupByCountQuery().Value,
        ];

    public async Task<long> SeedAsync()
    {
        await EmptyAsync();
        await using IAsyncSession session = Driver.AsyncSession();
        foreach (string statement in SchemaStatements)
        {
            IResultCursor created = await session.RunAsync(statement);
            _ = await created.ConsumeAsync();
        }
        IReadOnlyList<QueryGraph> graphs =
        [
            .. Queries.Select(query => QueryGraphs.Of(query, Guid.NewGuid())),
        ];
        await session.ExecuteWriteAsync(async runner =>
        {
            IResultCursor marked = await runner.RunAsync(
                $"CREATE (:{MarkerLabel} {{value: 'marker'}})"
            );
            _ = await marked.ConsumeAsync();
            foreach (QueryGraph graph in graphs)
            {
                await new InsertedGraph(runner, graph).ApplyAsync();
            }
        });
        return graphs.Sum(graph => (long)graph.Nodes.Count);
    }

    public async Task EmptyAsync()
    {
        await using IAsyncSession session = Driver.AsyncSession();
        IResultCursor cleared = await session.RunAsync("MATCH (n) DETACH DELETE n");
        _ = await cleared.ConsumeAsync();
        foreach (string name in await SchemaAsync())
        {
            string kind = name == "pureql_query_id" ? "CONSTRAINT" : "INDEX";
            IResultCursor dropped = await session.RunAsync(
                $"DROP {kind} {name} IF EXISTS"
            );
            _ = await dropped.ConsumeAsync();
        }
    }

    public Task<long> QueryNodeCountAsync()
    {
        return ScalarAsync("MATCH (n:Node) RETURN count(n)");
    }

    public Task<long> RelationshipCountAsync()
    {
        return ScalarAsync("MATCH ()-[r]->() RETURN count(r)");
    }

    public Task<long> MarkerCountAsync()
    {
        return ScalarAsync($"MATCH (n:{MarkerLabel}) RETURN count(n)");
    }

    /// <summary>
    /// The names of the constraints and of the indexes not backing one, except the
    /// token lookup indexes every database has.
    /// </summary>
    public async Task<IReadOnlyList<string>> SchemaAsync()
    {
        await using IAsyncSession session = Driver.AsyncSession();
        IResultCursor constraints = await session.RunAsync(
            "SHOW CONSTRAINTS YIELD name RETURN name"
        );
        List<string> names =
        [
            .. await constraints.ToListAsync(x => x["name"].As<string>()),
        ];
        IResultCursor indexes = await session.RunAsync(
            "SHOW INDEXES YIELD name, type, owningConstraint "
                + "WHERE type <> 'LOOKUP' AND owningConstraint IS NULL RETURN name"
        );
        names.AddRange(await indexes.ToListAsync(x => x["name"].As<string>()));
        return [.. names.Order(StringComparer.Ordinal)];
    }

    private async Task<long> ScalarAsync(string text)
    {
        await using IAsyncSession session = Driver.AsyncSession();
        IResultCursor cursor = await session.RunAsync(text);
        IRecord record = await cursor.SingleAsync();
        return record[0].As<long>();
    }
}
