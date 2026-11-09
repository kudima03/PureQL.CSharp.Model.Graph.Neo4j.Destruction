# Changelog

All notable changes to `PureQL.CSharp.Model.Graph.Neo4j.Destruction` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0-preview.0.1.0] - 2026-11-09

First release: drops every stored PureQL query graph and the Neo4j schema that
`PureQL.CSharp.Model.Graph.Neo4j.Creation` creates.

### Added

- **`DroppedSchemaOf`** — `sealed record DroppedSchemaOf(INeo4jStoredQueries
  Store)`: `ApplyAsync(ct)` opens a write session on the store's database and
  runs `DroppedSchema` on it, so each batch and each drop commits on its own.
  This is the recommended form.
- **`DroppedSchema`** — `sealed record DroppedSchema(IAsyncQueryRunner
  Runner)`: `ApplyAsync(ct)` deletes every `:Node` in batches of `BatchSize`
  (10000) until none is left, then runs the four drop statements. It is not
  atomic: a failed or cancelled run leaves what it already deleted deleted, and
  running it again finishes the job. Neo4j refuses schema statements in an
  explicit transaction that wrote data, so run it on a session, not inside such
  a transaction. Dropping an empty or schema-less database is a no-op.
- **`SchemaDestructionStatements`** — the four idempotent drop statements:
  `DROP CONSTRAINT pureql_query_id IF EXISTS`, then `DROP INDEX … IF EXISTS`
  for `pureql_node_query_id`, `pureql_node_field_source` and
  `pureql_node_param`.
- **`SchemaNames`** — the names of that constraint and those indexes, which
  must match the ones Creation gives them.
- Depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`,
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and
  `Neo4j.Driver` `6.5.1`; targets .NET 11 and is AOT-compatible.
