using System.Collections;
using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests;

public sealed record SchemaDestructionStatementsTests
{
    [Fact]
    public void DropsConstraintThenIndexes()
    {
        Assert.Equal(
            [
                "DROP CONSTRAINT pureql_query_id IF EXISTS",
                "DROP INDEX pureql_node_query_id IF EXISTS",
                "DROP INDEX pureql_node_field_source IF EXISTS",
                "DROP INDEX pureql_node_param IF EXISTS",
            ],
            new SchemaDestructionStatements().Select(x => x.Text)
        );
    }

    [Fact]
    public void HasNoParameters()
    {
        Assert.All(new SchemaDestructionStatements(), x => Assert.Empty(x.Parameters));
    }

    [Fact]
    public void EnumeratesNonGenerically()
    {
        IEnumerator enumerator = (
            (IEnumerable)new SchemaDestructionStatements()
        ).GetEnumerator();
        int count = 0;
        while (enumerator.MoveNext())
        {
            _ = Assert.IsType<DriverQuery>(enumerator.Current);
            count++;
        }
        Assert.Equal(4, count);
    }

    [Fact]
    public void EnumeratesRepeatedly()
    {
        SchemaDestructionStatements statements = new SchemaDestructionStatements();
        Assert.Equal(statements.Select(x => x.Text), statements.Select(x => x.Text));
    }

    [Fact]
    public void EqualsAnotherInstance()
    {
        Assert.Equal(
            new SchemaDestructionStatements(),
            new SchemaDestructionStatements()
        );
    }
}
