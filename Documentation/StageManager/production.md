---
title: Run Chronicle with a Production
description: "Run Chronicle in a cluster with a Stage Manager Production: TLS, external or managed MongoDB, SQL storage, an optional Neo4j graph and sinks."
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

### The System database name

Chronicle 19.117 renamed the databases named `System` to `_system`, and the kernel refuses to start when the System event store is empty under the configured name but holds events under `System`. An installation that predates it sets `database.systemDatabaseName: System` to keep its data where it is. Leave it empty for a new installation.

### Managed

A `Managed` database is a `PerconaServerMongoDB` run by the Percona operator, with these defaults:

| Setting | Default | Meaning |
| --- | --- | --- |
| `members` | `1` | `1` for development, `3` or `5` for production. One member is accepted through the Percona operator's replica-set-size flag |
| `oneMemberPerNode` | `true` | Run each member on its own node. Set `false` on a single-node cluster, such as kind |
| `memoryLimit` | `8Gi` | The memory limit of each member |
| `memoryRequest`, `cpuRequest`, `cpuLimit` | none | What each member requests, and its CPU limit. A member that requests nothing is the first evicted and shares CPU by weight |
| `wiredTigerCacheSizeRatio` | the operator's | The WiredTiger cache as a share of the memory limit minus 1 GB, above 0 and below 1. Percona's default of 0.5 gives a small cache at a small limit |
| `tcmallocReleaseRate` | `16777216` | Bytes per second at which MongoDB returns freed allocator memory to the operating system. MongoDB 8.0 never releases it when this is 0, and members were measured holding 2.4-3.3 GiB of free pages on top of the cache. 0 leaves MongoDB's default |
| `setParameters` | none | Further `mongod` `setParameter` values, by name. Integers and booleans are written as such, anything else as a string |
| `storageSize`, `storageClassName` | `10Gi`, the cluster's default | The volume of each member |
| `nodeSelector` | none | The node labels the members must match |
| `image` | `percona/percona-server-mongodb:7.0.14-8` | Pinned. Automatic upgrades are off |

TLS between the members is disabled: when cert-manager rotates the certificate authority, the Percona operator otherwise restarts the replica set in a loop. The members are only reachable inside the cluster.

Stage Manager waits for the Percona operator to report the database `ready`, then composes Chronicle's connection from the database administrator the Percona operator created into `<name>-mongodb-connection`.

`metrics.enabled` runs one `percona/mongodb_exporter` (`metrics.image`, pinned by default) per member, as a Deployment of its own so that adding it never rolls a `mongod`. Each connects directly to its own member as the Percona-managed `clusterMonitor` user, read from the users Secret into the environment, and serves `/metrics` on port 9216 with the `prometheus.io/*` scrape annotations on its pod. Only the diagnostic data and replica set status collectors run, because Chronicle creates a database per event store and namespace and per-database series would grow without bound. `metrics.nodeSelector` places them. Turning `metrics.enabled` off later leaves the Deployments in place; delete them.

`backup.enabled` runs the Percona Backup for MongoDB agent (`backup.image`, pinned by default) next to every member. Without a storage it only makes the database ready to be backed up.

`backup.storage` attaches the object storage the agent writes physical backups to. It uses the PBM `minio` storage type, which works against any S3 compatible storage; PBM's own `s3` type fails against some providers (UpCloud Object Storage answers its uploads with `XAmzContentSHA256Mismatch`). The bucket is addressed by path, so `endpoint` needs no wildcard DNS.

| Setting | Meaning |
| --- | --- |
| `bucket` | The bucket. Empty attaches no storage |
| `endpoint` | The storage endpoint, with or without `http://` or `https://`; without a scheme it is reached over TLS |
| `region` | The region of the bucket; empty leaves it to the storage |
| `prefix` | Where the backups are kept in the bucket; `pbm` by default |
| `credentialsSecretName` | A Secret in the Production's namespace with the keys `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` |
| `tasks` | `name`, `schedule` (cron) and `keep` for each schedule; empty uses hourly (24 kept), daily (7), weekly (4) and monthly (12) |

A storage needs a Managed database with `backup.enabled`. The Production stays `Blocked` until the credentials Secret has both keys.

## Vault

`vault.mode` says where Chronicle keeps the keys of the personal data it encrypts:

| Mode | Meaning |
| --- | --- |
| `None` (default) | In Chronicle's database. A backup of the database then brings back keys that were erased, so use a Vault when erasure must survive restores |
| `Managed` | A HashiCorp Vault that Stage Manager runs, named `<name>-vault` |
| `External` | A Vault you run: `vault.address` and a token in the Secret `vault.tokenSecretRef`, under the key `token` unless `key` is named |

Chronicle is configured for Vault compliance storage automatically, and always gets its token from a Secret (`VAULT_TOKEN`), never as a value. A Production does not start Chronicle until its Vault is ready (`VaultReady`).

### Moving to a Vault

A Production that ran with `mode: None`, or a restore of one, holds its keys in the database. Set `vault.migrateFromDefaultStorage: true` and Chronicle keeps serving those keys and copies each into Vault as it is read, so nothing is lost on the way. Remove it once every key has been read through.

### Managed Vault

`vault.managed.tier` chooses the shape:

| Tier | Nodes | Storage | Token of Chronicle |
| --- | --- | --- | --- |
| `Development` (default) | 1 | file | the root token |
| `Production` | 3 | Raft, so one node can be lost | a periodic token whose policy reaches only Chronicle's key paths (`secret/data/k1/*` and the few paths the Chronicle documentation lists), renewed hourly |

`storageSize` (default `1Gi`), `storageClassName` and `image` (pinned by default) are settings too. KV v2 is mounted at `secret/`.

A helper container next to every node initializes Vault once (the first node), keeps the unseal or recovery key and the root token in the Secret `<name>-vault-keys`, keeps the node unsealed, and writes the token Chronicle uses to `<name>-vault-chronicle` as its last step. The helper runs as a ServiceAccount of its own whose Role only reads and creates Secrets. Protect `<name>-vault-keys` like the Vault itself: whoever can read it can read every key.

`vault.managed.unseal.mode` chooses how Vault is unsealed whenever it starts:

| Mode | Meaning |
| --- | --- |
| `KubernetesSecret` (default) | The helper unseals each node with the key in `<name>-vault-keys` |
| `CloudKeyService` | A cloud key service unseals Vault: `cloud.provider` is `Aws` (`region`, `kmsKeyId`), `Azure` (`vaultName`, `keyName`, optionally `tenantId`) or `Google` (`project`, `region`, `keyRing`, `cryptoKey`), in `cloud.settings`. `cloud.credentialsSecretName` is a Secret whose keys become the provider's environment variables; leave it empty to use workload identity |
| `TransitVault` | A second Vault outside the cluster unseals it: `transit.address`, `transit.keyName`, `transit.mountPath` (default `transit`) and `transit.tokenSecretRef` |

```yaml
vault:
  mode: Managed
  managed:
    tier: Production
    unseal:
      mode: CloudKeyService
      cloud:
        provider: Aws
        settings:
          region: eu-west-1
          kmsKeyId: arn:aws:kms:eu-west-1:111122223333:key/example
        credentialsSecretName: vault-aws
```

With a cloud key service or a transit Vault, Vault is initialized with a recovery key, which the helper keeps in `<name>-vault-keys`. Settings that end up in the Vault configuration may only hold letters, digits and `. _ : / @ = , + - ` and spaces.

Only a token is supported for an external Vault, because Chronicle authenticates with `VAULT_TOKEN`.

## Federation

`federation` makes the Chronicle of a Production take part in [federation](https://github.com/Cratis/Chronicle/blob/main/Documentation/hosting/configuration/federation.md). Nothing is on by default.

| Field | Meaning |
| --- | --- |
| `enabled`, `port` | Opens the federation listener (`Federation:Enabled`), on port `35100` by default, in the container and in a `federation` port of Chronicle's Service. The listener presents Chronicle's certificate |
| `hosted.enabled`, `hosted.publicEndpoint`, `hosted.trustDomain` | Makes Chronicle the certificate authority customers enroll with. Needs `enabled`. The host of `publicEndpoint` is added to Chronicle's certificate |
| `dial.enabled`, `dial.eventStore`, `dial.namespace`, `dial.logicalSource` | Dials the authority this Chronicle enrolled with (`Federation:Dial`). It opens no listener and no port |
| `dial.serve`, `dial.serveEventTypes` | Also produces this Chronicle's public events to the authority over the same connection: a Production with only `dial` set is the sending side of a one-way link. Empty `serveEventTypes` serves every public event type |
| `dial.enrollmentCodeSecretRef` | The Secret holding the enrollment code, under the key `code` unless `key` is named. Mounted as a file |

```yaml
federation:
  dial:
    enabled: true
    eventStore: ledger
    logicalSource: direct
    serve: true
    enrollmentCodeSecretRef:
      name: direct-enrollment
```

The authority still needs a policy that lets this Chronicle in; that is configured on the authority, not here.

## Scheduling and resources

For a cluster that dedicates nodes to the Production, `scheduling` puts Chronicle, a Managed Vault, a Managed graph and the members of a Managed database on them. `database.nodeSelector` and `graph.nodeSelector` win for their own pods. Both are off by default.

```yaml
spec:
  scheduling:
    nodeSelector:
      workload: data
    tolerations:
      - key: workload
        operator: Equal
        value: data
        effect: NoSchedule
  resources:            # the Chronicle container
    requests: { cpu: 500m, memory: 1Gi }
    limits: { memory: 1536Mi }
```

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
| `database`, `graph`, `vault` | The database kind and mode, the graph mode and edition, and the vault mode and tier, the Production runs with |
| `conditions` | `SpecValid`, `VaultSupported`, `DatabaseModeUnchanged`, `GraphModeUnchanged`, `VaultModeUnchanged`, `CertManagerInstalled`, `SinkConfigured.<name>` per sink, `FederationConfigured` when the Production dials out and its enrollment code is missing, `CertificatesReady`, `DatabaseReady`, `GraphReady`, `VaultReady` with a Vault, `ConnectionAvailable` and `ChronicleReady` |

A Production is never half configured. An invalid spec, a missing Secret, a missing operator or a refused change makes it `Blocked`, and nothing is applied until it is resolved.

## Changes that are refused

Changing `database.kind`, `database.mode`, `graph.mode`, `graph.edition`, `vault.mode` or `vault.managed.tier` on a Production that runs would leave its data or its keys behind, so it is refused: the Production is `Blocked` with `DatabaseModeUnchanged`, `GraphModeUnchanged` or `VaultModeUnchanged` false, naming the field and what it runs with. Set the field back, or create a new Production.

## Performances check and read their sinks

A Performance on a Production lists the sinks its application uses in `application.sinks`. It is `Blocked` with the condition `SinksMapped` false, naming each sink the Production does not configure, before Chronicle would refuse the registration at run time. See [Run an application with a Performance](./performance.md#sinks).

The Production also gives the application the read side of what it uses, so queries read where the kernel writes:

| Sink or graph | What the application gets |
| --- | --- |
| A `MongoDB` sink | Its `connectionSecretRef` key, mounted as `/run/secrets/sinks/<name>`, and `Cratis__MongoDB__Sinks__<name>__ServerFile` naming it |
| The graph of the Production | The `Cratis__Neo4j__…` settings of Cratis.Arc.Neo4j, when the application sets `application.graph` or uses a Neo4j sink with `graphRef: managed` |
| A `PostgreSql` or `SqlServer` sink, or a Neo4j sink on another server | Nothing: Cratis Arc cannot read it. The condition `SinksReadable` is false as a warning, and the Performance still runs |

The Performance mounts these Secrets from its own namespace, so create it in the namespace of the Production. When one is not there, it is `Blocked` with `SinksBound` or `GraphBound` false, naming the Secret; nothing is copied between namespaces. Reading a named MongoDB sink needs the Cratis Arc release with [Cratis/Arc#3770](https://github.com/Cratis/Arc/issues/3770).
