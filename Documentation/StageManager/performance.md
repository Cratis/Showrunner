---
title: Run an application with a Performance
description: Deploy a Cratis application with a Stage Manager Performance, preparing environment secrets, Chronicle connections, AuthProxy and optional Ante onboarding.
---

A `Performance` is one application in one Cratis environment. It runs the application, its AuthProxy when the application has `authentication`, and its own Ante when the application has `onboarding`: exactly one Ante per Performance, not one per Chronicle or per Production.

Prepare the [environment and secrets](./environments-and-secrets.md) before using the manifest. The example names an environment ConfigMap and an existing Chronicle connection Secret; it does not create those inputs. A passphrase environment also needs the referenced passphrase Secret. For onboarding, prepare the legal ConfigMap with `terms.md` and `privacy.md`, and supply `ante.mongodb.server` in the environment.

The keys needed per provider kind are the ones AuthProxy's application contract requires: `tenantId`, `clientId` and `clientSecret` for `EntraId`; `authority`, `clientId` and `clientSecret` for `OpenId`; `clientId` and `clientSecret` for `GitHub`, `Google` and `Apple`. Each is addressed as `authentication.providers.<identity>.<key>`.

## Which Chronicle

Start with an existing Chronicle connection Secret named by `chronicle.connectionSecretRef`. It takes precedence over `productionRef`, as shown in the example below.

`productionRef` reads the connection Secret name from the Production's status, once the Production has written it; see [Run Chronicle with a Production](./production.md). Put the Performance in the namespace of its Production: the connection Secret is mounted from the Performance's own namespace. Either reference mounts the connection Secret into the workloads as a file; it is never copied.

```yaml
apiVersion: showrunner.cratis.io/v1alpha1
kind: Performance
metadata:
  name: crm
  namespace: apps
spec:
  # Either the Production whose Chronicle the application uses ...
  productionRef:
    name: ledger
  # ... or a connection Secret you already have. This takes precedence.
  chronicle:
    connectionSecretRef:
      name: chronicle-connection     # key defaults to connectionString
  application:
    image: registry.example.com/crm:1.0.0
    eventStores: [Crm]               # the first one is the store Ante observes
    sinks: [reporting]               # named sinks; needs productionRef
    graph: true                      # reads the Production's graph; needs productionRef
  environment:
    name: production
    configMapRef:
      name: crm-environment          # holds production.yaml, the environment file
    passphraseSecretRef:             # only for a passphrase environment
      name: crm-passphrase
  authProxy:
    enabled: true
    providers:
      - kind: EntraId
        identity: EntraId
      - kind: GitHub
        identity: GitHub
  onboarding:
    enabled: true
    signUp: CreateTenant
    personas: [Administrator, Member]
    legal:
      configMapRef:
        name: crm-legal              # terms.md and privacy.md
  ingress:
    enabled: true
    className: nginx
    hosts: [crm.example.com]
    tlsIssuer:
      name: letsencrypt              # a cert-manager ClusterIssuer
```

## Sinks

`application.sinks` lists the named sinks the application stores read models and graphs in. Each name must be configured by the Production named in `productionRef`, which maps it to MongoDB, PostgreSQL, SQL Server or Neo4j; see [Production storage and named sinks](./production.md). Names match without regard to case.

With a `productionRef`, the condition `SinksMapped` says whether the Production configures every sink the application uses. When it lacks one, the Performance is `Blocked`, nothing is applied, and the message names each missing sink. `application.sinks` needs `productionRef`. A published Stage application does not take it: its package declares its sinks in its manifest.

### Reading named sinks

Chronicle writes a read model to the sink its `[Sink("name")]` names. For the application's queries to read it from the same place, the operator binds each sink the application uses to Cratis Arc, which picks the sink from the same attribute:

- **MongoDB.** The key of the sink's `connectionSecretRef` is mounted as a file, `/run/secrets/sinks/<name>`, and `Cratis__MongoDB__Sinks__<name>__ServerFile` names it. The connection string is never an environment value. This needs the Cratis Arc release with [Cratis/Arc#3770](https://github.com/Cratis/Arc/issues/3770); an older Arc ignores the setting and reads those read models from its default database, where they are not.
- **PostgreSQL and SQL Server.** Cratis Arc cannot read a named SQL sink, so nothing is bound. The kernel still writes there, and queries over those read models would read the default storage instead. The condition `SinksReadable` is false, naming the sink, as a warning: the Performance still rolls out.
- **Neo4j.** Cratis.Arc.Neo4j reads one graph per application, so only the graph of the Production can be bound; a Neo4j sink on another server is named by `SinksReadable` as well.

The condition `SinksBound` says whether the MongoDB sinks are bound. Their Secrets are in the namespace of the Production, and a Performance mounts Secrets from its own namespace only: when one is not there, the Performance is `Blocked` with `SinksBound` false, naming the Secret, and nothing is copied across namespaces.

### Reading the graph

Set `application.graph: true`, or use a Neo4j sink with `graphRef: managed`, and the operator binds the graph of the Production to Cratis.Arc.Neo4j through its `Cratis:Neo4j` section:

| Variable | Value |
| --- | --- |
| `Cratis__Neo4j__Server` | The bolt address of a managed graph, as a plain value: it is not a secret. For an external graph it comes from the `server` key of `graph.credentialsSecretName` through a `secretKeyRef` |
| `Cratis__Neo4j__Password` | The `password` key of the graph's Secret, through a `secretKeyRef` |
| `Cratis__Neo4j__UserName` | The user Chronicle writes with |
| `Cratis__Neo4j__DatabaseMode`, `Cratis__Neo4j__DatabasePrefix` | `Single` on Community; `PerNamespace` and the prefix on Enterprise, as the kernel writes them |
| `Cratis__Neo4j__EventStore` | The first of `application.eventStores` |

Cratis.Arc.Neo4j takes its settings as values and has no file-based setting, so the password is a `secretKeyRef` environment variable: the Deployment names the Secret, never the value. The application reads with the credential Chronicle writes with; on Community there is only one user, and the condition `GraphBound` says so with the reason `SharedCredential`. `GraphBound` is false and the Performance `Blocked` when the Production has no graph or its Secret is not in the namespace of the Performance.

## What is created

For a Performance named `crm`:

| Resource | Name | Notes |
| --- | --- | --- |
| Deployment and Service | `crm-app` | The application. ClusterIP |
| Deployment and Service | `crm-authproxy` | When `authProxy.enabled`. ClusterIP |
| Deployment and Service | `crm-ante` | When `onboarding.enabled`. One replica, ClusterIP |
| NetworkPolicy | `crm-app` | For a published Stage application: only AuthProxy of the Performance may connect to the application |
| Ingress | `crm` | When `ingress.enabled`. Every host routes to AuthProxy and nothing else, with a certificate from cert-manager |
| Secrets | `crm-app-environment`, `crm-authproxy-environment`, `crm-ante-secrets`, `crm-signing-keys` | What is mounted into each workload, and the generated keys |

AuthProxy is the only public entry point. The application and Ante are ClusterIP Services behind it.

Ante gets its own event store, `ante-<name>` by default, so two Performances on one Production never share one. Its MongoDB server comes from the environment key `ante.mongodb.server`.

## Never half-configured

Nothing is created or changed until everything the Performance needs is in hand: a valid spec, the Chronicle connection, every sink the application uses, the resolved environment and every environment key the providers and onboarding need. When something is missing the phase is `Blocked`, the missing keys are listed in `status.missingEnvironmentKeys`, a condition says why, and the Deployments that already run are left as they are.

Use the condition to identify the missing input, supply the referenced resource or environment key, then inspect the status again.

## Status

| Field | Meaning |
| --- | --- |
| `phase` | `Pending`, `Blocked`, `Progressing`, `Ready` or `Expired` |
| `publicUrl` | `https://` and the first ingress host |
| `missingEnvironmentKeys` | The keys the environment lacks, while the phase is `Blocked` |
| `conditions` | `SpecValid`, `ChronicleConnection`, `SinksMapped`, `SinksReadable` and `SinksBound` (with `productionRef`), `GraphBound` (when the application reads the graph), `EnvironmentResolved`, `EnvironmentComplete`, and `ApplicationReady`, `AuthProxyReady`, `AnteReady` per component |
| `signingKeys` | The key ids in use, the rotation counter applied and how many previous public keys are pinned |
| `runningVersion`, `versions`, `rollout` | The version of a [published Stage application](./published-stage-applications.md) that runs, its history and the rollout under way |
| `expiresAt`, `dataRetainedUntil` | When a [lifetime](./published-stage-applications.md#lifetime) ends, and until when the publisher should keep the data |
| `resourceUsage`, `currentPeriodUsage`, `lastPeriodUsage` | CPU and memory of the pods, accumulated from `metrics.k8s.io` |

After the workloads are applied, `Progressing` means one or more enabled component Deployments are not ready; `Ready` means they are all ready. Use `ApplicationReady`, `AuthProxyReady` and `AnteReady` to see which enabled component is still waiting. `publicUrl` reports the ingress address; it does not confirm DNS, TLS or sign-in.

Conditions and the status never contain a secret value.

## Application, lifetime and usage

`application.published` runs the application as a published Stage application instead of an image you built; see [Run a published Stage application](./published-stage-applications.md). It is an alternative application path, not a further step in deploying an ordinary image.

`lifetime` (`permanent: true`, or `until`) applies to every Performance: at `until` the workloads are scaled to zero, the Ingress is removed and the phase is `Expired`. The shared [lifetime reference](./published-stage-applications.md#lifetime) covers retention and resuming a Performance as well as expiry. Usage is sampled from the metrics API on every reconciliation, which is every 30 seconds for a published application.

## Signing keys

The operator generates the key Ante signs invitations with and the key AuthProxy signs attestations with, and keeps them in `crm-signing-keys`. Raise `spec.signingKeys.rotation` to rotate both. The public half of each replaced key stays pinned (`signingKeys.retainPrevious`, two by default): Ante verifies invitations signed with a previous key, and Ante's attestation trust lists every pinned AuthProxy key.

## Onboarding values the operator derives

The Performance spec carries the providers and the onboarding behavior; the values come from the environment. The operator derives the rest of both contracts so nobody writes them twice: the invitation issuer and audience (`urn:cratis:ante:<store>:<namespace>`), the AuthProxy attestation issuer and audience, the public URLs and the generated keys. With `onboarding.attestedProviders` set, Ante runs attested invitation exchange against AuthProxy's attestations; without it, Ante keeps the legacy exchange.

AuthProxy forwards every request it lets through to the application's Service, `<name>-app`: the operator sets `Cratis__AuthProxy__Services__application__Backend__BaseUrl` and `…Frontend__BaseUrl` to it, on `application.port` (9090 for a published application).

AuthProxy's onboarding URLs are on the public host, as its contract requires HTTPS outside development. Routing the `/_invite` and `/.well-known/ante` paths from AuthProxy to Ante is configured on AuthProxy's side; set `onboarding.lobbyBackendBaseUrl` in the environment when your AuthProxy needs it.
