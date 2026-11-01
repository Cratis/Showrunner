---
title: Stage operator
description: The Stage operator is a Kubernetes operator that runs published Stage application versions permanently or for a period, with persistent storage and sign-in.
---

The Stage operator runs published Stage applications. Stage runs an application as a disposable sandbox; the operator runs an exported version of it as a real application for real users, permanently or until a given time.

A publisher, such as Studio, creates a [`StageApplication`](./resource-reference.md) resource. The operator reconciles it into:

- a `Deployment` with one pod that runs two containers: the exported **Stage** image in its published mode, and the **sign-in proxy** in front of it;
- a `StatefulSet` with a volume for the application's **MongoDB**, created on the first publish and kept across versions;
- a `Service` that exposes only the proxy port, an `Ingress` for `<publishedName>.<baseDomain>` with TLS, and `NetworkPolicy` objects;
- a **status** that tells the publisher the phase, the running version, the URL, the expiry, the rollout history and the resource usage.

The operator is a .NET application, delivered as the `ghcr.io/cratis/showrunner-stage-operator` image and the `stage-operator` Helm chart, which installs the custom resource definition with it.

## What the operator does

| Behavior | How |
| --- | --- |
| Run a version | Applies the generated objects with server-side apply, so reconciling is idempotent and repairs drift. |
| Roll forward | A change of `spec.image` or `spec.version` is a rolling update: the new pod must be ready before the old one goes. The data volume is kept. |
| Roll back | A version that is not ready within `RolloutTimeout`, or that Kubernetes gives up on, is rolled back to the previous image. The history records `RolledBack`, and the failed version stays held back until the spec asks for another one. |
| Honor the lifetime | At `spec.lifetime.until` the application is scaled to zero, the ingress is removed and the phase is `Expired`. The data is kept for `ExpiredDataRetention`, then removed. |
| Delete | Deleting the resource stops the application at once and keeps its data for `ExpiredDataRetention` (a finalizer holds the resource), then removes everything. |
| Refuse | A resource without `spec.authentication` is refused (`Failed`, reason `AuthenticationMissing`) and nothing is created. |
| Report usage | Samples `metrics.k8s.io` pod metrics for every container of the application every `MetricsSampleInterval` into `status.resourceUsage` and the last completed UTC day into `status.lastPeriodUsage`. |

## Install

```shell
helm install stage-operator oci://ghcr.io/cratis/charts/stage-operator \
  --namespace stage-operator --create-namespace \
  --set operator.baseDomain=apps.example.invalid \
  --set operator.tlsIssuerAnnotation=letsencrypt
```

Helm installs the definition in the chart's `crds` folder once and never upgrades it. After upgrading the chart, apply the new definition yourself:

```shell
kubectl apply -f Deployment/Helm/stage-operator/crds/stageapplications.showrunner.cratis.io.yaml
```

The cluster needs the Kubernetes metrics API (`metrics-server`) for resource usage; without it the operator runs normally and reports `measured: false`.

See [The StageApplication resource](./resource-reference.md), [Configuration](./configuration.md) and [Security boundary](./security-boundary.md).
