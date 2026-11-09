---
title: Run Chronicle with a Production
description: Run Chronicle in a cluster with a Stage Manager Production, with TLS, an external or managed MongoDB, SQL storage, an optional Neo4j graph and the named sinks of its applications.
---

A `Production` is Chronicle for one or more applications. Stage Manager turns it into a running Chronicle: a StatefulSet behind a headless Service, its TLS certificate, its database, an optional Neo4j graph, and a connection Secret that applications use and nothing else. Each application on it keeps its own event stores.

## Before you start

A Production needs these in the cluster, besides [the operator](./install.md):

- **cert-manager**, which issues Chronicle's TLS certificate. Without it the Production is `Blocked` with the condition `CertManagerInstalled` false.
- **The Percona Operator for MongoDB**, only for `database.mode: Managed`. Without it a managed Production is `Blocked` with `DatabaseReady` false and the reason `PerconaOperatorNotInstalled`.
- The Secrets the spec names, in the namespace of the Production.

Create the Production in a namespace listed in the chart's `managedNamespaces`, and the Performances that use it in the same namespace: a Performance mounts the connection Secret from its own namespace.

## A Production on an external MongoDB

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: ledger-mongo
  namespace: apps
stringData:
  connectionString: mongodb://chronicle:<password>@mongo.example.internal:27017/?replicaSet=rs0
---
apiVersion: showrunner.cratis.io/v1alpha1
kind: Production
metadata:
  name: ledger
  namespace: apps
spec:
  version: 19.148.0          # the tag of the cratis/chronicle production image
  replicas: 1
  database:
    kind: MongoDB
    mode: External
    connectionSecretName: ledger-mongo
```

`version` is a tag of the production image, `cratis/chronicle`, not of the `-development` images. Changing it rolls the StatefulSet one replica at a time.

## What Stage Manager creates

Every resource is named after the Production and owned by it, so deleting the Production removes them. The volumes of a managed database or graph are kept.

| Resource | Name | What it is |
| --- | --- | --- |
| StatefulSet | `<name>-chronicle` | Chronicle, `replicas` of it, from `cratis/chronicle:<version>` |
| Service | `<name>-chronicle` | Headless. Its port `35000` is named `chronicle`, so DNS answers `_chronicle._tcp.<name>-chronicle.<namespace>.svc.cluster.local` and `chronicle+srv://` finds every ready replica |
| Issuer, Certificate | `<name>-selfsigned`, `<name>-ca` | A self-signed certificate authority for the Production |
| Certificate | `<name>-chronicle-tls` | Chronicle's certificate, written as a PKCS#12 keystore for Chronicle and with `ca.crt` for clients |
| Secret | `<name>-chronicle-keystore` | The generated keystore password |
| Secret | `<name>-chronicle-credentials` | The generated administrator password and the client applications connect as |
| Secret | `<name>-chronicle-connection` | The connection Secret, reported in `status.connectionSecretName` |
| PerconaServerMongoDB, Secret | `<name>-mongodb`, `<name>-mongodb-connection` | A managed database and the connection Chronicle reads from it |
| StatefulSet, Service, Secret | `<name>-neo4j` | A managed graph, its address and its generated password |

Secrets that hold a generated value are written once and kept: the administrator, the client and the graph keep their credentials across reconciliations and restarts of the operator.

Chronicle is applied only once its certificate is issued and its database is ready, so it never starts without them.

## Connect an application

The connection Secret holds everything an application needs, and an application needs nothing else:

| Key | Holds |
| --- | --- |
| `connectionString` | `chronicle+srv://<client>:<secret>@<name>-chronicle.<namespace>.svc.cluster.local` |
| `clientId`, `clientSecret` | The client Chronicle bootstraps for applications |
| `ca.crt` | The certificate authority Chronicle's certificate chains to |

A Performance uses it through `productionRef`; see [Run an application with a Performance](./performance.md). Any other application mounts the Secret and reads `connectionString`. To validate Chronicle's certificate, mount `ca.crt` as well, set `Cratis__Chronicle__Tls__CaCertificatePath` to its path and add `skipTlsValidation=false` to the connection string.

`status.chronicleAddress` is the same address without credentials.

## Replicas and versions

One replica runs with Chronicle's default single-node clustering. Above one, the replicas form one Orleans cluster through the database: `MongoDB` clustering on MongoDB storage and `Sql` clustering on PostgreSQL or SQL Server, with the Production's name as the cluster and service id and each pod advertising its own address. Scaling from 1 to 3 restarts the replica it has and starts two more in the same cluster.

## Database

`database.kind` is the engine Chronicle stores its events in:

| Kind | Mode | Chronicle storage type |
| --- | --- | --- |
| `MongoDB` (default) | `Managed` or `External` | `MongoDB` |
| `PostgreSql` | `External` only | `PostgreSql` |
| `SqlServer` | `External` only | `MsSql` |

### External

An `External` database takes its connection string from the Secret named by `database.connectionSecretName`, under the key `connectionString`. Stage Manager creates no database resource for it.

### Managed

A `Managed` database is a `PerconaServerMongoDB` run by the Percona operator, with these defaults:

| Setting | Default | Meaning |
| --- | --- | --- |
| `members` | `1` | `1` for development, `3` or `5` for production. One member is accepted through the Percona operator's replica-set-size flag |
| `oneMemberPerNode` | `true` | Run each member on its own node. Set `false` on a single-node cluster, such as kind |
| `memoryLimit` | `8Gi` | The memory limit of each member |
| `storageSize`, `storageClassName` | `10Gi`, the cluster's default | The volume of each member |
| `nodeSelector` | none | The node labels the members must match |
| `image` | `percona/percona-server-mongodb:7.0.14-8` | Pinned. Automatic upgrades are off |

TLS between the members is disabled: when cert-manager rotates the certificate authority, the Percona operator otherwise restarts the replica set in a loop. The members are only reachable inside the cluster.

Stage Manager waits for the Percona operator to report the database `ready`, then composes Chronicle's connection from the database administrator the Percona operator created into `<name>-mongodb-connection`.

## Graph

`graph` gives the Production a Neo4j that graph projections land in.

| `graph.mode` | What happens |
| --- | --- |
| `None` (default) | No graph. The kernel's Neo4j settings are left unset |
| `Managed` | Neo4j Community in a StatefulSet, with a persistent volume and a generated password |
| `External` | A Neo4j server elsewhere. `graph.credentialsSecretName` names a Secret with its bolt URI under `server` and its password under `password`; `graph.userName` is the user, `neo4j` by default. No Neo4j resource is created |

`graph.edition` is `Community` (default) or `Enterprise`:

- **Community** has one user database, so the kernel stores every graph of every event store in it (`databaseMode: Single`). A managed graph is always Community.
- **Enterprise** gives each event store and namespace its own database (`databaseMode: PerNamespace`), prefixed by `graph.databasePrefix`, the name of the Production by default, so several Productions can share one server.

A managed graph takes `storageSize` (`10Gi`), `storageClassName`, `nodeSelector` and `image` (`neo4j:5.26.0-community`, pinned), as a managed database does.

The Neo4j password is never written into the spec, the status, a condition or an environment value: it is mounted from its Secret. The condition `GraphReady` says whether the graph is ready.

## Named sinks

Applications can name the sink a read model or graph is stored in. The model only names the sink; the Production maps each name to storage, the way an environment maps its keys.

```yaml
spec:
  database:
    kind: MongoDB
    mode: Managed
  graph:
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
        graphRef: managed          # the Production's managed graph, or a connectionSecretRef
```

| Field | Meaning |
| --- | --- |
| `readModels` | The sink read models are stored in when they name none. Must be `MongoDB`, `PostgreSql` or `SqlServer` |
| `graphs` | The sink graphs are stored in when they name none. Must be `Neo4j` |
| `configurations[].name` | The name applications use: letters and digits, separated by single hyphens or underscores. Unique without regard to case |
| `configurations[].kind` | `MongoDB`, `PostgreSql` or `SqlServer` for read models, `Neo4j` for graphs |
| `configurations[].connectionSecretRef` | The Secret, in the namespace of the Production, holding the connection string, or the bolt URI of a Neo4j server |
| `configurations[].graphRef` | A Neo4j sink only: `managed`, the Production's own graph, which needs `graph.mode: Managed` |

Each configured sink has a condition, `SinkConfigured.<name>`. A duplicate name, an unknown kind, a SQL or MongoDB sink without `connectionSecretRef`, a Neo4j sink without exactly one of `connectionSecretRef` and `graphRef`, and a Secret that does not exist or lacks its key each make that condition false with a message naming the sink, and the Production `Blocked`.

## The kernel configuration

From a valid spec, Stage Manager derives Chronicle's configuration:

| Setting | From |
| --- | --- |
| `Cratis__Chronicle__Storage__Type` | `database.kind` |
| `Cratis__Chronicle__Storage__ConnectionDetailsFile` | The database's connection, mounted as a file |
| `Cratis__Chronicle__Neo4j__ServerFile`, `…__PasswordFile` | The graph's bolt URI and password, mounted as files |
| `Cratis__Chronicle__Neo4j__UserName`, `…__DatabaseMode`, `…__DatabasePrefix` | The graph's user, `Single` or `PerNamespace`, and the prefix on Enterprise |
| `Cratis__Chronicle__Sinks__ReadModels`, `Cratis__Chronicle__Sinks__Graphs` | `sinks.readModels`, `sinks.graphs` |
| `Cratis__Chronicle__Sinks__Configurations__<name>__Type` | `MongoDB`, `SQL` for `PostgreSql` and `SqlServer`, or `Neo4j` |
| `Cratis__Chronicle__Sinks__Configurations__<name>__ConnectionDetailsFile` | A MongoDB or SQL sink's connection, mounted as a file |
| `Cratis__Chronicle__Sinks__Configurations__<name>__ServerFile` | A Neo4j sink's bolt URI, mounted as a file. A sink on the managed graph also gets its `PasswordFile`, `UserName` and `DatabaseMode` |

A Production with no `sinks` and no graph sets only the storage type and its connection file.

Connections are never written into the spec, the status or an environment value. Each is mounted from its Secret into `/run/secrets/chronicle`, as a file named after the setting it holds, such as `/run/secrets/chronicle/Cratis__Chronicle__Storage__ConnectionDetails`. The kernel reads the file through the matching `…File` setting, once, at startup. Stage Manager sets only the `…File` form, never the value itself: Chronicle refuses to start when both are set.

The Neo4j `ServerFile` settings, and `PasswordFile` on the top-level `neo4j` section, need the Chronicle release that contains [Cratis/Chronicle#5771](https://github.com/Cratis/Chronicle/issues/5771). An older kernel ignores them and finds no graph.

The keystore password, the administrator password and the client secret reach Chronicle as environment variables that reference their Secrets, so their values never stand in the StatefulSet.

## Status

| Field | Meaning |
| --- | --- |
| `phase` | `Blocked` when nothing is applied, `Progressing` while something is not ready, `Ready` when Chronicle, its database and its graph are |
| `chronicleAddress` | The `chronicle+srv://` address of Chronicle |
| `connectionSecretName` | The connection Secret, once the certificate authority is issued |
| `database`, `graph` | The database kind and mode, and the graph mode and edition, the Production runs with |
| `conditions` | `SpecValid`, `VaultSupported`, `DatabaseModeUnchanged`, `GraphModeUnchanged`, `CertManagerInstalled`, `SinkConfigured.<name>` per sink, `CertificatesReady`, `DatabaseReady`, `GraphReady`, `ConnectionAvailable` and `ChronicleReady` |

A Production is never half configured. An invalid spec, a missing Secret, a missing operator or a refused change makes it `Blocked`, and nothing is applied until it is resolved.

## Changes that are refused

Changing `database.kind`, `database.mode`, `graph.mode` or `graph.edition` on a Production that runs would leave its data behind, so it is refused: the Production is `Blocked` with `DatabaseModeUnchanged` or `GraphModeUnchanged` false, naming the field and what it runs with. Set the field back, or create a new Production.

`vault.mode` other than `None` is not supported yet ([#17](https://github.com/Cratis/Showrunner/issues/17)); the Production is `Blocked` with `VaultSupported` false.

## Performances check and read their sinks

A Performance on a Production lists the sinks its application uses in `application.sinks`. It is `Blocked` with the condition `SinksMapped` false, naming each sink the Production does not configure, before Chronicle would refuse the registration at run time. See [Run an application with a Performance](./performance.md#sinks).

The Production also gives the application the read side of what it uses, so queries read where the kernel writes:

| Sink or graph | What the application gets |
| --- | --- |
| A `MongoDB` sink | Its `connectionSecretRef` key, mounted as `/run/secrets/sinks/<name>`, and `Cratis__MongoDB__Sinks__<name>__ServerFile` naming it |
| The graph of the Production | The `Cratis__Neo4j__…` settings of Cratis.Arc.Neo4j, when the application sets `application.graph` or uses a Neo4j sink with `graphRef: managed` |
| A `PostgreSql` or `SqlServer` sink, or a Neo4j sink on another server | Nothing: Cratis Arc cannot read it. The condition `SinksReadable` is false as a warning, and the Performance still runs |

The Performance mounts these Secrets from its own namespace, so create it in the namespace of the Production. When one is not there, it is `Blocked` with `SinksBound` or `GraphBound` false, naming the Secret; nothing is copied between namespaces. Reading a named MongoDB sink needs the Cratis Arc release with [Cratis/Arc#3770](https://github.com/Cratis/Arc/issues/3770).
