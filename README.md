# PureQL.CSharp.Model.Graph.Neo4j.Destruction

Drops the Neo4j schema and every stored query graph of the **PureQL** graph storage packages.

[![.NET build & test](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/PureQL.CSharp.Model.Graph.Neo4j.Destruction)](https://www.nuget.org/packages/PureQL.CSharp.Model.Graph.Neo4j.Destruction)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

`PureQL.CSharp.Model.Graph.Neo4j.Destruction` is the operation package of the `PureQL.CSharp.Model.Graph` family that undoes `PureQL.CSharp.Model.Graph.Neo4j.Creation` and everything stored since: it deletes every stored query graph (every `:Node` with its relationships), then drops the constraint and the indexes Creation creates. Every step is idempotent, so dropping an empty or schema-less database is a no-op. Nodes without the `Node` label and other schema objects are left in place.

## Public API

Namespace `PureQL.CSharp.Model.Graph.Neo4j.Destruction`:

| Name | Description |
|------|-------------|
| `DroppedSchemaOf(INeo4jStoredQueries Store)` | `ApplyAsync(ct)`: opens a write session on `Store.Driver` for `Store.Database` and runs `DroppedSchema` on it. The recommended form |
| `DroppedSchema(IAsyncQueryRunner Runner)` | `ApplyAsync(ct)`: deletes the nodes in batches of `BatchSize` (10000), then runs `SchemaDestructionStatements`, on the given runner |
| `SchemaNames` | The constraint and index names Creation creates: `pureql_query_id`, `pureql_node_query_id`, `pureql_node_field_source`, `pureql_node_param` |
| `SchemaDestructionStatements` | `IEnumerable<Neo4j.Driver.Query>`: `DROP CONSTRAINT pureql_query_id IF EXISTS`, then `DROP INDEX <name> IF EXISTS` for the three indexes |

The nodes are deleted by repeating, until it deletes none:

```cypher
MATCH (n:Node) WITH n LIMIT $batch DETACH DELETE n RETURN count(n) AS deleted
```

so that no transaction grows with the number of stored queries. The operation is not atomic: if it fails or is cancelled midway, what was deleted stays deleted, and running it again finishes the job.

In Neo4j, schema statements and data writes cannot share an explicit transaction: `DroppedSchema` on an `IAsyncTransaction` that deleted nodes fails on the first drop statement. Run it on a session (each statement then commits on its own), or use `DroppedSchemaOf`.

This package does not reference the Creation package: `SchemaNames` duplicates its names, which must match.

## Installation

```
dotnet add package PureQL.CSharp.Model.Graph.Neo4j.Destruction
```

## Usage

```csharp
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using PureQL.CSharp.Model.Graph.Neo4j.Destruction;

await using IDriver driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
INeo4jStoredQueries store = new Neo4jStoredQueries(driver, "neo4j");

await new DroppedSchemaOf(store).ApplyAsync();

// Or on a session of your own:
await using IAsyncSession session = driver.AsyncSession();
await new DroppedSchema(session).ApplyAsync();
```
