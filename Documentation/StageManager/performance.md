---
title: Run an application with a Performance
description: A Performance runs one application and environment, with AuthProxy and its own Ante, reachable only through AuthProxy.
---

A `Performance` is one application in one Cratis environment. It runs the application, its AuthProxy when the application has `authentication`, and its own Ante when the application has `onboarding`: exactly one Ante per Performance, not one per Chronicle or per Production.

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

## Which Chronicle

`productionRef` reads the connection Secret name from the Production's status, which the Production reconciliation fills in once it exists. Until then, or when you run your own Chronicle, name an existing Secret with `chronicle.connectionSecretRef`. Either way the Secret is mounted into the workloads as a file; it is never copied.

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

Nothing is created or changed until everything the Performance needs is in hand: a valid spec, the Chronicle connection, the resolved environment and every environment key the providers and onboarding need. When something is missing the phase is `Blocked`, the missing keys are listed in `status.missingEnvironmentKeys`, a condition says why, and the Deployments that already run are left as they are.

The keys needed per provider kind are the ones AuthProxy's application contract requires: `tenantId`, `clientId` and `clientSecret` for `EntraId`; `authority`, `clientId` and `clientSecret` for `OpenId`; `clientId` and `clientSecret` for `GitHub`, `Google` and `Apple`. Each is addressed as `authentication.providers.<identity>.<key>`.

## Status

| Field | Meaning |
| --- | --- |
| `phase` | `Pending`, `Blocked`, `Progressing`, `Ready` or `Expired` |
| `publicUrl` | `https://` and the first ingress host |
| `missingEnvironmentKeys` | The keys the environment lacks, while the phase is `Blocked` |
| `conditions` | `SpecValid`, `ChronicleConnection`, `EnvironmentResolved`, `EnvironmentComplete`, and `ApplicationReady`, `AuthProxyReady`, `AnteReady` per component |
| `signingKeys` | The key ids in use, the rotation counter applied and how many previous public keys are pinned |
| `runningVersion`, `versions`, `rollout` | The version of a [published Stage application](./published-stage-applications.md) that runs, its history and the rollout under way |
| `expiresAt`, `dataRetainedUntil` | When a [lifetime](./published-stage-applications.md#lifetime) ends, and until when the publisher should keep the data |
| `resourceUsage`, `currentPeriodUsage`, `lastPeriodUsage` | CPU and memory of the pods, accumulated from `metrics.k8s.io` |

Conditions and the status never contain a secret value.

## Application, lifetime and usage

`application.published` runs the application as a published Stage application instead of an image you built; see [Run a published Stage application](./published-stage-applications.md). `lifetime` (`permanent: true`, or `until`) applies to every Performance: at `until` the workloads are scaled to zero, the Ingress is removed and the phase is `Expired`. Usage is sampled from the metrics API on every reconciliation, which is every 30 seconds for a published application.

## Signing keys

The operator generates the key Ante signs invitations with and the key AuthProxy signs attestations with, and keeps them in `crm-signing-keys`. Raise `spec.signingKeys.rotation` to rotate both. The public half of each replaced key stays pinned (`signingKeys.retainPrevious`, two by default): Ante verifies invitations signed with a previous key, and Ante's attestation trust lists every pinned AuthProxy key.

## Onboarding values the operator derives

The Performance spec carries the providers and the onboarding behavior; the values come from the environment. The operator derives the rest of both contracts so nobody writes them twice: the invitation issuer and audience (`urn:cratis:ante:<store>:<namespace>`), the AuthProxy attestation issuer and audience, the public URLs and the generated keys. With `onboarding.attestedProviders` set, Ante runs attested invitation exchange against AuthProxy's attestations; without it, Ante keeps the legacy exchange.

AuthProxy forwards every request it lets through to the application's Service, `<name>-app`: the operator sets `Cratis__AuthProxy__Services__application__Backend__BaseUrl` and `…Frontend__BaseUrl` to it, on `application.port` (9090 for a published application).

AuthProxy's onboarding URLs are on the public host, as its contract requires HTTPS outside development. Routing the `/_invite` and `/.well-known/ante` paths from AuthProxy to Ante is configured on AuthProxy's side; set `onboarding.lobbyBackendBaseUrl` in the environment when your AuthProxy needs it.
