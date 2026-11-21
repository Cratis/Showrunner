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
| Deployment and Service | `crm-app` | The application. ClusterIP. Not created for a [gateway](#run-only-authproxy-in-front-of-a-service) |
| Deployment and Service | `crm-authproxy` | When `authProxy.enabled`. ClusterIP |
| Deployment and Service | `crm-ante` | When `onboarding.enabled`. One replica, ClusterIP |
| NetworkPolicy | `crm-app` | For a published Stage application: only AuthProxy of the Performance may connect to the application |
| Ingress | `crm` | When `ingress.enabled`. Every host routes to AuthProxy and nothing else, with a certificate from cert-manager. `ingress.annotations` are added to it |
| Secrets | `crm-app-environment`, `crm-authproxy-environment`, `crm-ante-secrets`, `crm-signing-keys` | What is mounted into each workload, and the generated keys |

AuthProxy is the only public entry point. The application and Ante are ClusterIP Services behind it.

Ante gets its own event store, `ante-<name>` by default, so two Performances on one Production never share one. Its MongoDB server comes from the environment key `ante.mongodb.server`.

## AuthProxy and Ante are optional

A Performance is only the application until you ask for more. Each part exists because its block is in the spec, and nothing else turns it on:

| Omit | What happens |
| --- | --- |
| `authProxy` (or `enabled: false`) | No AuthProxy runs. The application is reachable only inside the cluster, so `ingress` and `onboarding` are refused with a clear message: AuthProxy is the only public entry point and the place invitations are exchanged. |
| `onboarding` | No Ante runs. AuthProxy gets no invitation settings and routes nothing to `/_invite` or `/.well-known/ante`. The environment needs no `ante.mongodb.server`. |
| `ingress` | No Ingress and no certificate. The workloads still run and can be reached by other workloads in the cluster. |

Every setting below is optional too. Leave one out and the operator writes nothing for it, so AuthProxy or Ante keeps its own default.

## Configure AuthProxy

The provider list says which providers exist; where their values come from is your choice. By default the client id and secret live in the [Cratis environment](./environments-and-secrets.md). Name a Kubernetes Secret with `credentialsSecretRef` and the operator reads the client id and the client secret from it instead, hands them to AuthProxy as mounted files and never puts them in a Deployment, the status or the log.

| Field | What it does |
| --- | --- |
| `authProxy.image`, `port`, `replicas`, `resources` | The container. `resources` takes `requests` and `limits` |
| `providers[].credentialsSecretRef` | `name`, and `clientIdKey` and `clientSecretKey` when the keys are not `clientId` and `clientSecret`. The environment then needs neither key |
| `providers[].scopes` | The scopes the provider is asked for, for example `read:user`, `user:email` and `read:org` for GitHub. They win over a `scopes` key in the environment |
| `tenancy.tenantId` | A fixed tenant for every request: a service that is not multi-tenant |
| `tenancy.tenantsEndpoint` | A URL that lists the tenants of the signed-in person. One tenant goes straight in, several are offered, none is refused. Give `tenantId` or `tenantsEndpoint`, not both |
| `requiredClaims[]` | A `claim` and the values of which the caller needs one (`anyOf`). Every entry must be satisfied. With GitHub and the `read:org` scope, `urn:github:organization` and `urn:github:team` (`org/team`) restrict a service to a team |
| `pages.configMapRef` | A ConfigMap of pages and assets, one file per key: `403.html`, `404.html`, `select-tenant.html`, `styles.css` and so on. It is mounted at `/mnt/pages` and AuthProxy's `PagesPath` points there. A page that is present replaces the built-in one |
| `dataProtection.claimName` | An existing PersistentVolumeClaim for AuthProxy's key ring, so a restart does not sign everyone out. It is `ReadWriteOnce`, so AuthProxy runs one replica and stops the old pod before it starts the new one |

With `onboarding`, AuthProxy also routes the invitation paths on the public host to Ante's Service, and the operator writes that address itself; set `onboarding.lobbyBackendBaseUrl` in the environment only to point it somewhere else.

## Configure Ante

Beyond the [contract the operator derives](#onboarding-values-the-operator-derives), `onboarding` takes how Ante looks and a few rules:

| Field | What it does |
| --- | --- |
| `signUp`, `registrationEnabled`, `registrationReview` | `JoinTenant` closes self-service registration, so people get in by invitation only. `registrationReview` holds a completed sign-up for the application's review |
| `allowedLocales`, `defaultLocale` | The locales Ante offers |
| `reservedOrganizationNames` | Organization names nobody can claim, compared ignoring case |
| `branding.logoUrl`, `customCssUrl` | A path on Ante's host, such as `/brand/logo.svg`, or an https URL |
| `branding.pageTitle`, `splashMessage` | The tab title, and the text announced while Ante loads |
| `branding.assets` | `configMapRef` and `files`: the keys of a ConfigMap Ante serves itself under `/brand/<key>`. Each key is its own mount, because a mounted folder exposes symbolic links that the file server reports with the wrong length |

The host store, the host application URL and the public address of the invitation links come from `application.eventStores[0]` and the first ingress host. Nothing to set.

## An application with AuthProxy and Ante

This Performance runs the application, signs people in with GitHub, restricts the application to a GitHub team, lets people in by invitation only, brands Ante and publishes everything on one host. Every key below is a field of the spec.

```yaml
apiVersion: showrunner.cratis.io/v1alpha1
kind: Performance
metadata:
  name: keldakraft
  namespace: keldakraft
spec:
  productionRef:
    name: keldakraft
  application:
    image: ghcr.io/kelda/keldakraft:1.2.3
    eventStores: [KeldaKraft]
  environment:
    name: production
    configMapRef:
      name: keldakraft-environment   # holds ante.mongodb.server and the application's own keys
  authProxy:
    enabled: true
    providers:
      - kind: GitHub
        identity: GitHub
        credentialsSecretRef:
          name: keldakraft-github    # keys clientId and clientSecret
        scopes: [read:user, user:email, read:org]
    tenancy:
      # The application's Service is keldakraft-app.
      tenantsEndpoint: http://keldakraft-app:8080/api/internal/tenants/selectable
    requiredClaims:
      - claim: urn:github:organization
        anyOf: [kelda]
      - claim: urn:github:team
        anyOf: [kelda/staff]
    pages:
      configMapRef:
        name: keldakraft-authproxy-pages
    dataProtection:
      claimName: keldakraft-authproxy-keys
    resources:
      requests:
        cpu: 100m
        memory: 128Mi
  onboarding:
    enabled: true
    signUp: JoinTenant
    personas: [Administrator, Member]
    allowedLocales: [en]
    reservedOrganizationNames: [admin, api, app]
    branding:
      logoUrl: /brand/logo.svg
      customCssUrl: /brand/lobby.css
      pageTitle: Kelda Kraft
      splashMessage: Loading Kelda Kraft
      assets:
        configMapRef:
          name: keldakraft-brand
        files: [logo.svg, lobby.css]
  ingress:
    enabled: true
    className: nginx
    hosts: [app.keldakraft.example.com]
    tlsIssuer:
      name: letsencrypt-prod
    annotations:
      # Observable queries are long-lived WebSockets that sit quiet for long stretches.
      nginx.ingress.kubernetes.io/proxy-read-timeout: "3600"
      nginx.ingress.kubernetes.io/proxy-send-timeout: "3600"
```

Register the callback `https://app.keldakraft.example.com/signin-github` with the GitHub OAuth app whose client id and secret are in `keldakraft-github`; a GitHub app has one callback, so each public host has its own. The organization must have approved the OAuth app, or its teams are never visible and `urn:github:team` never matches.

## Run only AuthProxy in front of a Service

Name an `authProxy.upstream` and the Performance is a gateway: it runs AuthProxy, the Ingress and nothing else. There is no application, no Ante and no Chronicle, so `application`, `onboarding`, `productionRef` and `chronicle` are refused next to it. This is how an operator console such as the Kubernetes dashboard, Grafana or the Chronicle Workbench gets the same sign-in and the same team restriction as the application.

```yaml
apiVersion: showrunner.cratis.io/v1alpha1
kind: Performance
metadata:
  name: keldakraft-operations
  namespace: keldakraft
spec:
  authProxy:
    enabled: true
    upstream:
      url: http://grafana.infrastructure.svc.cluster.local:3000/
      thirdParty: true               # the default: Grafana has no /.cratis/me to ask who is calling
    providers:
      - kind: GitHub
        identity: GitHub
        credentialsSecretRef:
          name: operations-github
        scopes: [read:user, user:email, read:org]
    tenancy:
      tenantId: Default
    requiredClaims:
      - claim: urn:github:organization
        anyOf: [kelda]
      - claim: urn:github:team
        anyOf: [kelda/staff]
  ingress:
    enabled: true
    className: nginx
    hosts: [operations.keldakraft.example.com]
    tlsIssuer:
      name: letsencrypt-prod
```

With `thirdParty: true` AuthProxy only decides who may get in and forwards to the upstream as a plain frontend. Set it to `false` for an upstream that is a Cratis service: AuthProxy then forwards to it as the backend too and asks it who is calling.

The Cratis environment is optional in a gateway. It is read only when `environment` is given or a provider has no `credentialsSecretRef`. Declare one gateway per console and host; the Performances share nothing but the namespace.

## Never half-configured

Nothing is created or changed until everything the Performance needs is in hand: a valid spec, the Chronicle connection, every sink the application uses, the resolved environment and every environment key the providers and onboarding need. When something is missing the phase is `Blocked`, the missing keys are listed in `status.missingEnvironmentKeys`, a condition says why, and the Deployments that already run are left as they are.

Use the condition to identify the missing input, supply the referenced resource or environment key, then inspect the status again.

## Status

| Field | Meaning |
| --- | --- |
| `phase` | `Pending`, `Blocked`, `Progressing`, `Ready` or `Expired` |
| `publicUrl` | `https://` and the first ingress host |
| `missingEnvironmentKeys` | The keys the environment lacks, while the phase is `Blocked` |
| `conditions` | `SpecValid`, `ChronicleConnection`, `SinksMapped`, `SinksReadable` and `SinksBound` (with `productionRef`), `GraphBound` (when the application reads the graph), `EnvironmentResolved`, `ProviderCredentialsResolved` (when a provider names `credentialsSecretRef`), `EnvironmentComplete`, and `ApplicationReady`, `AuthProxyReady`, `AnteReady` per component |
| `signingKeys` | The key ids in use, the rotation counter applied and how many previous public keys are pinned |
| `runningVersion`, `versions`, `rollout` | The version of a [published Stage application](./published-stage-applications.md) that runs, its history and the rollout under way |
| `expiresAt`, `dataRetainedUntil` | When a [lifetime](./published-stage-applications.md#lifetime) ends, and until when the publisher should keep the data |
| `resourceUsage`, `currentPeriodUsage`, `lastPeriodUsage` | CPU and memory of the pods, accumulated from `metrics.k8s.io` |

After the workloads are applied, `Progressing` means one or more enabled component Deployments are not ready; `Ready` means they are all ready. Use `ApplicationReady`, `AuthProxyReady` and `AnteReady` to see which enabled component is still waiting. `publicUrl` reports the ingress address; it does not confirm DNS, TLS or sign-in.

Conditions and the status never contain a secret value.

## The pod of the application

These `application` settings shape the pod of an application you built. They do not apply to a published application, whose pod Stage Manager shapes itself; naming one there is a `SpecValid` problem.

| Field | Meaning |
| --- | --- |
| `strategy` | `RollingUpdate` starts the new pod before it stops the old one. `Recreate` stops the old pod first, so two never share state: use it for an application that hosts a single-replica silo. Empty keeps the cluster's default |
| `imagePullSecrets` | The names of the Secrets the image is pulled with, for the application and its init containers |
| `probes` | `readinessPath`, `livenessPath` and `startupPath`, HTTP paths starting with `/`. Without `readinessPath` readiness checks the port; without the others there is no such check. Liveness and readiness wait for startup, for up to five minutes |
| `resources` | `requests` and `limits` by resource name, such as `cpu: 250m` and `memory: 512Mi`. None are set when it is left out |
| `nodeSelector` | The node labels the pods must match |
| `volumes`, `volumeMounts` | A volume is an `EmptyDir`, or an existing `PersistentVolumeClaim` named by `claimName`; Stage Manager never creates or deletes the claim, so its data outlives the Performance. A mount names the `volume`, the absolute `path` and whether it is `readOnly` |
| `initContainers` | Containers that run to completion, in order, before the application starts. Each has a `name`, an `image` (the application's by default), `command`, `args`, plain `environment` and `volumeMounts`. They get no Secret from the Performance: put no secret in `environment` |
| `egress` | The destinations the pods may connect to, as rules of `ports` and a `cidr`, a `namespace`, or neither for every public address except the private ranges. Naming any rule isolates the pods for egress: they then reach DNS, their own namespace and these destinations only. It is a NetworkPolicy named `<name>-egress`, deleted when the rules are removed. Egress rules apply to a published application too |

```yaml
application:
  image: registry.example.com/crm:1.0
  strategy: Recreate
  imagePullSecrets: [registry]
  probes:
    startupPath: /health/startup
    livenessPath: /health/live
    readinessPath: /health/ready
  volumes:
    - name: chronicle-trust
  volumeMounts:
    - volume: chronicle-trust
      path: /trust
      readOnly: true
  initContainers:
    - name: import-trust
      command: ["/bin/sh", "-c", "cp /etc/ssl/certs/* /trust"]
      volumeMounts:
        - volume: chronicle-trust
          path: /trust
  egress:
    - ports: [587]
```

An application that runs more than one component, such as an ingest or processing worker next to the application, is one Performance per component, all with the same `productionRef`. Each is its own workload with its own image, environment and lifetime. A Performance without `authProxy` or `onboarding` runs the application alone.

## Application, lifetime and usage

`application.published` runs the application as a published Stage application instead of an image you built; see [Run a published Stage application](./published-stage-applications.md). It is an alternative application path, not a further step in deploying an ordinary image.

`lifetime` (`permanent: true`, or `until`) applies to every Performance: at `until` the workloads are scaled to zero, the Ingress is removed and the phase is `Expired`. The shared [lifetime reference](./published-stage-applications.md#lifetime) covers retention and resuming a Performance as well as expiry. Usage is sampled from the metrics API on every reconciliation, which is every 30 seconds for a published application.

## Signing keys

The operator generates the key Ante signs invitations with and the key AuthProxy signs attestations with, and keeps them in `crm-signing-keys`. Raise `spec.signingKeys.rotation` to rotate both. The public half of each replaced key stays pinned (`signingKeys.retainPrevious`, two by default): Ante verifies invitations signed with a previous key, and Ante's attestation trust lists every pinned AuthProxy key.

## Onboarding values the operator derives

The Performance spec carries the providers and the onboarding behavior; the values come from the environment. The operator derives the rest of both contracts so nobody writes them twice: the invitation issuer and audience (`urn:cratis:ante:<store>:<namespace>`), the AuthProxy attestation issuer and audience, the public URLs and the generated keys. With `onboarding.attestedProviders` set, Ante runs attested invitation exchange against AuthProxy's attestations; without it, Ante keeps the legacy exchange.

AuthProxy forwards every request it lets through to the application's Service, `<name>-app`: the operator sets `Cratis__AuthProxy__Services__application__Backend__BaseUrl` and `…Frontend__BaseUrl` to it, on `application.port` (9090 for a published application).

AuthProxy's onboarding URLs are on the public host, as its contract requires HTTPS outside development. The operator also routes the `/_invite` and `/.well-known/ante` paths from AuthProxy to Ante's in-cluster Service, by setting `Cratis__AuthProxy__Invite__Lobby__Backend__BaseUrl` on AuthProxy: the contract key `onboarding.lobbyBackendBaseUrl` only accepts HTTPS, so it is not used. Set that key in the environment only to send the paths to another address; it wins.
