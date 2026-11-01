---
title: Configuration
description: The settings of the Stage operator, in appsettings, environment variables and the Helm chart.
---

The operator reads the `StageOperator` section of `appsettings.json`, overridden by `StageOperator__<Key>` environment variables. The Helm chart sets them from the `operator` values.

| Key | Helm value | Default | Meaning |
| --- | --- | --- | --- |
| `BaseDomain` | `operator.baseDomain` | `cratis.app` | Applications are published at `<publishedName>.<BaseDomain>`. |
| `IngressClassName` | `operator.ingressClassName` | empty | The ingress class. Empty leaves the cluster default. |
| `TlsIssuerAnnotation` | `operator.tlsIssuerAnnotation` | empty | The cert-manager cluster issuer; sets `cert-manager.io/cluster-issuer` so each host gets a certificate in the secret `<name>-tls`. |
| `TlsSecretName` | `operator.tlsSecretName` | empty | A wildcard secret that exists in each application namespace. When set it is used instead of a certificate per host. |
| `ExpiredDataRetention` | `operator.expiredDataRetention` | `7.00:00:00` | How long the data of an expired or deleted application is kept. |
| `WatchNamespaces` | `operator.watchNamespaces` | empty | Namespaces to watch. Empty watches all and installs a `ClusterRole`; listed namespaces get a `Role` each. |
| `RolloutTimeout` | `operator.rolloutTimeout` | `00:05:00` | How long a new image gets to become ready before the rollback. |
| `MetricsSampleInterval` | `operator.metricsSampleInterval` | `00:00:30` | How often pod metrics are sampled. |
| `ResyncInterval` | `operator.resyncInterval` | `00:01:00` | How often every resource is reconciled even without an event. |
| `MongoImage` | `operator.mongoImage` | `mongo:8` | The image of the per-application database. |
| `StorageClassName` | `operator.storageClassName` | empty | The storage class of the database volumes. |
| `DefaultStorageSize` | `operator.defaultStorageSize` | `5Gi` | The volume size when `spec.storage.size` is empty. |
| `NotesCollectorPort` | `operator.notesCollectorPort` | `0` | The port of the Notes collector that is allowed into application pods when Notes is enabled. `0` allows none. |
| `DenyPathsWithSnippet` | `operator.denyPathsWithSnippet` | `false` | Also add an `ingress-nginx` `server-snippet` that answers the denied paths. See [Security boundary](./security-boundary.md). |

The operator runs as a single replica. The chart uses the `Recreate` strategy so two operators never reconcile the same resources.

## Operating notes

- The storage size of an existing volume is not changed; the `volumeClaimTemplate` of a `StatefulSet` is immutable. Resize the claim in the cluster if the storage class allows it.
- The operator needs no access to secrets. The kubelet reads the secrets a pod references.
- Run it locally against a cluster with `dotnet run --project Source/DotNET/StageOperator`; it uses the current kubeconfig context outside a pod.
