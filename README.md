# PureQL.CSharp.Model.Graph.Neo4j.Destruction

Drops the Neo4j schema and every stored query graph of the **PureQL** graph storage packages.

[![.NET build & test](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/PureQL.CSharp.Model.Graph.Neo4j.Destruction)](https://www.nuget.org/packages/PureQL.CSharp.Model.Graph.Neo4j.Destruction)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

`PureQL.CSharp.Model.Graph.Neo4j.Destruction` is the operation package of the `PureQL.CSharp.Model.Graph` family that undoes `PureQL.CSharp.Model.Graph.Neo4j.Creation`: it drops the constraint and the indexes Creation creates. Every statement is idempotent, so dropping a schema that does not exist is a no-op.

## Public API

Namespace `PureQL.CSharp.Model.Graph.Neo4j.Destruction`:

| Name | Description |
|------|-------------|
| `SchemaNames` | The constraint and index names Creation creates: `pureql_query_id`, `pureql_node_query_id`, `pureql_node_field_source`, `pureql_node_param` |
| `SchemaDestructionStatements` | `IEnumerable<Neo4j.Driver.Query>`: `DROP CONSTRAINT pureql_query_id IF EXISTS`, then `DROP INDEX <name> IF EXISTS` for the three indexes |

This package does not reference the Creation package: `SchemaNames` duplicates its names, which must match.

## Installation

```
dotnet add package PureQL.CSharp.Model.Graph.Neo4j.Destruction
```
