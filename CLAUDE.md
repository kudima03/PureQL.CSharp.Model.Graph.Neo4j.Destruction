# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All `dotnet` commands must be run from the `./src` directory. The SDK is pinned by `global.json` (.NET 11, for the model's C# 15 unions); where the system SDK is older, use `~/.dotnet/dotnet` or put it first on `PATH` with `DOTNET_ROOT=$HOME/.dotnet`.

```bash
dotnet restore
dotnet build --no-restore -warnaserror
dotnet format --verify-no-changes          # check code style (CI enforces this)
dotnet format                              # auto-fix code style
dotnet test --no-build --verbosity normal  # run tests
dotnet csharpier check .                   # check formatting (CI enforces this)
dotnet pack --configuration Release -p:Version=<version> --output .
```

CI additionally passes `-p:AssemblyVersion` (pinned to the major) and `-p:FileVersion`; see `.github/workflows/publish-nuget.yml`.

## Architecture

This is the **Destruction package** of the `PureQL.CSharp.Model.Graph` family: it deletes every stored query graph and drops the constraint and the indexes that `PureQL.CSharp.Model.Graph.Neo4j.Creation` creates. It depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`, `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and `Neo4j.Driver` `6.5.1`, and on no other operation package. Namespace `PureQL.CSharp.Model.Graph.Neo4j.Destruction`, one public type per file:

- `SchemaNames` — the constant names of the constraint (`pureql_query_id`) and the indexes (`pureql_node_query_id`, `pureql_node_field_source`, `pureql_node_param`). They duplicate the Creation package's names and must match them.
- `SchemaDestructionStatements` — `sealed record : IEnumerable<Neo4j.Driver.Query>`: `DROP CONSTRAINT … IF EXISTS`, then `DROP INDEX … IF EXISTS` for each index.
- `DroppedSchema(IAsyncQueryRunner Runner)` — `ApplyAsync(ct)` repeats the internal `NodeBatchDeletionStatement` (`MATCH (n:Node) WITH n LIMIT $batch DETACH DELETE n RETURN count(n) AS deleted`) until it returns 0, then runs `SchemaDestructionStatements`. `BatchSize` (10000) is public; an internal constructor takes another size for tests. Neo4j refuses schema statements in an explicit transaction that wrote data, so it is meant for a session, where each statement is auto-commit.
- `DroppedSchemaOf(INeo4jStoredQueries Store)` — opens a write session on `Store.Driver` for `Store.Database` and runs `DroppedSchema` on it. Not atomic: a failed or cancelled run leaves what it already deleted deleted.

Only `:Node` nodes are deleted: every node of a query graph has that label, and other nodes (such as Migration's version marker) and other schema objects stay.

Inside the `PureQL.CSharp.Model` namespace tree, `Query` names the model's subquery union, so code here aliases the driver type (`using DriverQuery = Neo4j.Driver.Query;`).

The public API is the shared contract of the family: do not rename or reshape it. Every public member has XML documentation (`GenerateDocumentationFile`; a missing comment fails the build under `-warnaserror`).

**Test project:** `PureQL.CSharp.Model.Graph.Neo4j.Destruction.Tests` targets `net11.0` and uses xunit with coverlet; unit tests check the names and the statements; integration tests start `neo4j:5-community` through Testcontainers.Neo4j (no fixed ports, one container shared by the `SharedNeo4j` collection, so tests run one at a time). `SeededDatabase` creates Creation's schema with its four statements plus an unrelated index, writes sample queries of `PureQL.CSharp.Model.Samples` with `QueryGraphs.Of` and a test-only UNWIND insert (`InsertedGraph`, duplicated from Insertion), and adds a non-`:Node` marker node. Docker is required to run them.

**Target framework:** net11.0 only, as the model. `IsAotCompatible` is `true`.

**Package validation:** enabled against `PackageValidationBaselineVersion` `0.1.0-preview.0.1.0`, the first release; the `nuget-publish` skill moves it after each release, and intentional breaks go to `CompatibilitySuppressions.xml` next to the csproj.

**Mutation testing:** the Stryker step of CI is commented out.

**Publishing:** triggered by pushing a semver tag (pattern `*.*.*`). The tag becomes the `PackageVersion`. The workflow publishes to both GitHub Packages and NuGet.org.

## Code Style

Enforced via `.editorconfig` and `dotnet format --verify-no-changes` in CI:

- No `var` — always use explicit types
- No expression-bodied methods or constructors — use block bodies
- Expression-bodied properties and accessors are required
- File-scoped namespace declarations (`namespace Foo.Bar;`)
- Private fields prefixed with `_` in camelCase (e.g., `_options`)
- Braces always required, even for single-statement blocks
- Max line length: 90 characters
- Allman brace style — opening brace on its own line for all blocks
- `System.*` using directives sorted first, no blank line between using groups
- Parentheses required in arithmetic and relational binary operators for clarity

## Commit Messages

Do not mention Claude or AI assistance in commit messages.
