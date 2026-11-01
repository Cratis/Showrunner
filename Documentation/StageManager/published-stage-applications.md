---
title: Run a published Stage application
description: Run an exported Stage application package on the generic Stage runtime image with a Performance - its package, storage, sign-in through AuthProxy, Notes, rollouts with rollback, lifetime and usage.
---

A published Stage application is the generic Stage runtime image (`cratis/stage:<runtime version>`) in published mode, with an application package mounted into it. A `Performance` runs one when `application.published` is set. Nothing is built per application: a new version is a new package ConfigMap, and the image changes only when the runtime does.

```yaml
apiVersion: showrunner.cratis.io/v1alpha1
kind: Performance
metadata:
  name: orders
  namespace: apps
spec:
  environment:
    name: production
    configMapRef:
      name: orders-environment       # the sign-in providers' values
  application:
    image: cratis/stage:5.3.0        # the Stage runtime, not an image per application
    published:
      version: 1.4.0
      packageConfigMapRef:
        name: orders-package-1-4-0   # immutable; binaryData key package.tar.gz
      storageConnectionSecretRef:
        name: orders-storage         # key connectionString
      trustedProxies: ["10.244.0.0/16"]
      notes:
        collectorUrl: https://collector.example.invalid:8443
        collectorTokenSecretRef:
          name: orders-notes-token   # key token
        publicationId: pub-7c1d
        collectorTenantId: acme-corp
        applicationName: Orders
  authProxy:
    enabled: true
    providers:
      - kind: GitHub
        identity: GitHub
  ingress:
    enabled: true
    hosts: [orders.cratis.app]
    tlsIssuer:
      name: letsencrypt
  lifetime:
    until: 2026-12-31T00:00:00Z      # or permanent: true; omit for permanent
```

A published application needs `authProxy.enabled` and `ingress.hosts` (Stage answers only the hosts it is published under) and runs one replica. It does not take a `chronicle` or `productionRef`: Stage hosts its own Chronicle kernel in the container. `onboarding` is not supported for it. Like every Performance it is never half applied: when the package, the storage Secret, the Notes token Secret or the environment is not there, the phase is `Blocked` and the conditions `PackageAvailable`, `StorageConnection`, `NotesCollectorToken` and `EnvironmentComplete` say which.

## The package

`packageConfigMapRef` names a ConfigMap in the namespace of the Performance whose `binaryData` key `package.tar.gz` (or `key`) holds the archive that Stage exports. It must be `immutable: true`; a version never changes in place, which is what makes a rollback to it safe. The ConfigMap is mounted read-only at `/stage/package-archive/` and `Stage__Published__PackagePath` points at the file. Name one ConfigMap per version, for example `orders-package-1-4-0`. A ConfigMap holds up to 1 MiB.

## Storage

The kernel inside Stage needs MongoDB. `storageConnectionSecretRef` names a Secret with the connection string (key `connectionString` unless set). It is projected into the container as a file and Stage reads it through `Stage__Published__Storage__ConnectionStringFile`; it is never copied, never an environment variable and never in the status. Give each application a database of its own in the connection string.

The operator does not run MongoDB for a published application. Stage Manager takes a connection from a Secret for the Chronicle of an ordinary Performance in the same way, and managed storage is the job of a Production, which is not reconciled yet. The publisher provisions the database and owns it, including when to remove it after the application expired.

## Sign-in and trust

AuthProxy is the only public entry point, as for every Performance. It signs users in and forwards each request to the application with the `x-ms-client-principal*` headers. Stage believes those headers only from the addresses in `trustedProxies` and strips them from any other caller.

AuthProxy runs as its own Deployment, so Stage sees the address of an AuthProxy pod: set `trustedProxies` to the pod network of the cluster (for example `10.244.0.0/16` on kind). That is safe because the operator also creates the NetworkPolicy `<name>-app`, which lets only the AuthProxy pods of the same Performance connect to Stage's port 9090. Nothing else in the cluster can reach Stage, and Chronicle's port 35000 is never exposed by a Service or the Ingress. The cluster needs a network plugin that enforces NetworkPolicy.

The Ingress routes every host to AuthProxy and nothing else, so `/scalar`, `/openapi`, `/workbench`, `/read-models`, `/stage/load` and `/stage/semantic` are not reachable from outside, and published mode does not map them in the first place.

The application's Service is `<name>-app` on port 9090. Stage Manager does not configure AuthProxy's backend address for any application; point AuthProxy at that Service.

## Notes

With `notes` set, the application hosts the Notes backend and sends notes over HTTP to the collecting host: `Notes__Transport=Http`, `Notes__CollectorUrl`, `Notes__CollectorTokenFile` (the Secret mounted at `/run/secrets/notes/token`), `Notes__PublicationId`, `Notes__CollectorTenantId` and `Notes__ApplicationName`. See [Transports](../Notes/transports.md). Leave `notes` out, or set `enabled: false`, for an application without Notes.

## Rollouts

A release is the application `version`, the runtime `image` and the package ConfigMap. Changing any of them rolls out. The application Deployment keeps the old pod serving until the new one is ready (`maxUnavailable: 0`, readiness on `/stage/status`).

- When the new release is ready, `status.versions` gets an entry with outcome `RolledOut` and `status.runningVersion` is the new version.
- When it is not ready within `rolloutTimeoutSeconds` (300), `status.versions` gets an entry for it with outcome `Failed` and a `reason`, another for the previous release with outcome `RolledBack`, and the previous image and package are deployed again. The condition `RolloutSucceeded` is `False`, `runningVersion` is the old version and the phase can be `Ready`.
- A release that failed is not tried again while the spec asks for it. Ask for something else, such as a new package ConfigMap, to try again.
- A first release that never becomes ready is recorded as `Failed` and stays deployed, because there is nothing to roll back to.

`status.versions` keeps the last twenty entries, the newest last. Stage's own data stays in the storage the Secret names.

Two Stage pods briefly run against one database during a rollout, because the old pod serves until the new one is ready.

## Lifetime

`lifetime` is either `permanent: true` or `until`, and applies to any Performance. When `until` passes:

1. The Ingress is deleted, so the public route is gone.
2. Every Deployment is scaled to zero.
3. The phase is `Expired`, the condition `Expired` is `True`, `publicUrl` is empty and `status.dataRetainedUntil` is `until` plus `dataRetentionDays` (7).

The operator owns no storage of a published application, so it removes no data. `dataRetainedUntil` tells the publisher when it may drop the database it provided. Moving `until` into the future runs the Performance again. Deleting the Performance removes everything it created through owner references.

## Usage

On every reconciliation (every 30 seconds for a published application) the operator reads `PodMetrics` for all pods of the Performance from `metrics.k8s.io` and adds them to the status, summed over all containers:

- `resourceUsage`: the total since measuring began, with `cpuCoreSeconds`, `peakMemoryBytes`, `averageMemoryBytes`, `windowStart`, `windowEnd`, `sampleCount` and `measured`.
- `currentPeriodUsage`: the same for the UTC day under way.
- `lastPeriodUsage`: the last completed UTC day, with `day` (`yyyy-MM-dd`). It is replaced when the next day completes, so a publisher that reports daily reads it once a day.

CPU is the rate each sample reports multiplied by the time since the previous sample, at most two minutes, so a gap while the operator was down is not billed. `measured` is `false` when the cluster has no metrics API; install metrics-server.

## Resources

The Stage container requests 250m CPU and 512Mi memory and is limited to 1 CPU and 1Gi memory. Set `application.published.resources.requests` and `limits` to change them.
