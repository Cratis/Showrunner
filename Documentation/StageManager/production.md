---
title: Production storage and named sinks
description: Describe the storage engine of a Stage Manager Production and the named sinks that map the read models and graphs of its applications to MongoDB, PostgreSQL, SQL Server or Neo4j.
---

A `Production` is Chronicle for one or more applications. Its spec says which engine Chronicle keeps its events in and where the read models and graphs of its applications are stored.

> Production reconciliation is [not implemented yet](https://github.com/Cratis/Showrunner/issues/15). The schema below can be created, read and validated, and a Performance checks its sinks against it, but nothing runs Chronicle for a Production, so no kernel configuration is applied yet.

## Storage engine

`database.kind` is the engine Chronicle stores its events in:

| Kind | Mode | Chronicle storage type |
| --- | --- | --- |
| `MongoDB` (default) | `Managed` or `External` | `MongoDB` |
| `PostgreSql` | `External` only | `PostgreSql` |
| `SqlServer` | `External` only | `MsSql` |

An `External` database takes its connection string from the Secret named by `database.connectionSecretName`, under the key `connectionString`. A `Managed` database is the Production's own MongoDB.

## Named sinks

Applications can name the sink a read model or graph is stored in. The model only names the sink; the Production maps each name to storage, the way an environment maps its keys.

```yaml
apiVersion: showrunner.cratis.io/v1alpha1
kind: Production
metadata:
  name: ledger
  namespace: apps
spec:
  version: 15.0.0
  database:
    kind: MongoDB
    mode: Managed
  sinks:
    readModels: reporting          # optional; empty keeps the database
    graphs: people                 # optional; empty keeps the graph of the Production
    configurations:
      - name: reporting
        kind: PostgreSql           # MongoDB | PostgreSql | SqlServer | Neo4j
        connectionSecretRef:
          name: reporting-pg       # key defaults to connectionString
      - name: people
        kind: Neo4j
        graphRef: managed          # the Production's own Neo4j, or a connectionSecretRef
```

| Field | Meaning |
| --- | --- |
| `readModels` | The sink read models are stored in when they name none. Must be `MongoDB`, `PostgreSql` or `SqlServer` |
| `graphs` | The sink graphs are stored in when they name none. Must be `Neo4j` |
| `configurations[].name` | The name applications use: letters and digits, separated by single hyphens or underscores. Unique without regard to case |
| `configurations[].kind` | `MongoDB`, `PostgreSql` or `SqlServer` for read models, `Neo4j` for graphs |
| `configurations[].connectionSecretRef` | The Secret, in the namespace of the Production, holding the connection string, or the bolt URI of a Neo4j server |
| `configurations[].graphRef` | A Neo4j sink only: the graph of the Production to use instead of a Secret |

A duplicate name, an unknown kind, a default naming a sink that is not configured, a Neo4j sink for read models, a SQL or MongoDB sink for graphs, a SQL or MongoDB sink without `connectionSecretRef`, and a Neo4j sink without exactly one of `connectionSecretRef` and `graphRef` are each a problem that names the sink.

## The kernel configuration

From a valid spec, Stage Manager derives Chronicle's `storage` and `sinks` configuration:

| Setting | From |
| --- | --- |
| `Cratis__Chronicle__Storage__Type` | `database.kind` |
| `Cratis__Chronicle__Sinks__ReadModels`, `Cratis__Chronicle__Sinks__Graphs` | `sinks.readModels`, `sinks.graphs` |
| `Cratis__Chronicle__Sinks__Configurations__<name>__Type` | `MongoDB`, `SQL` for `PostgreSql` and `SqlServer`, or `Neo4j` |
| `Cratis__Chronicle__Storage__ConnectionDetails` | The external database's Secret, mounted as a file |
| `Cratis__Chronicle__Sinks__Configurations__<name>__ConnectionDetails` | A MongoDB or SQL sink's Secret, mounted as a file |
| `Cratis__Chronicle__Sinks__Configurations__<name>__Server` | A Neo4j sink's Secret, mounted as a file |

Connections are never written into the spec, the status or an environment value. Each is mounted from its Secret into `/run/secrets/chronicle`, as a file named after the setting it holds. The Chronicle kernel reads its settings from `chronicle.json` and environment variables only, so it cannot read those files yet; applying this configuration needs Chronicle to read a setting from a mounted file.

## Performances check their sinks

A Performance on a Production lists the sinks its application uses in `application.sinks`. It is `Blocked` with the condition `SinksMapped` false, naming each sink the Production does not configure, before Chronicle would refuse the registration at run time. See [Run an application with a Performance](./performance.md#sinks).
