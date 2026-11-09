---
title: Install the operator
description: Install the Stage Manager Helm chart and its prerequisites, configure secret-provider identities and review the operator's Kubernetes permissions.
---

The operator is published as a container image, `cratis/showrunner-operator-image`, and as a Helm chart, `cratis/showrunner-operator`, of the same version, so a chart always installs the operator it was released with. They live in separate repositories because a registry keeps one artifact per tag: a chart pushed under the image's tag would replace the image.

Before installing, have Helm available and access to the target Kubernetes cluster with permission to install cluster-scoped CRDs and RBAC, as well as the operator's namespaced resources. If your environment uses Azure Key Vault or HashiCorp Vault, prepare the operator's identity and provider access using the chart values below.

The chart installs the custom resource definitions from its `crds` folder, the operator Deployment and its permissions. Helm installs CRDs once and does not upgrade them, so apply the `crds` folder of a new chart yourself when you upgrade.

```shell
helm install stage-manager oci://registry-1.docker.io/cratis/showrunner-operator \
  --namespace stage-manager --create-namespace \
  --set 'managedNamespaces={apps}'
```

List in `managedNamespaces` every namespace you create Performances or Productions in, and create those namespaces before installing. The operator can read and write Secrets, ConfigMaps and the workloads of a Performance only there; a Performance in any other namespace cannot be prepared. Leave the list empty to manage Performances in the release namespace only.

After installation, use your cluster's inspection tools to check that the CRDs are present and the operator Deployment is available. These are installation checks; they do not establish that an application is ready.

## Prerequisites of Productions

Performances need nothing more. A [Production](./production.md) relies on operators the chart does not install:

- **cert-manager**, for Chronicle's TLS certificate. Every Production needs it.
- **The Percona Operator for MongoDB**, for a Production with `database.mode: Managed`. A Production on an external database does not need it.

Install them in the cluster before creating a Production. A Production whose cluster lacks one is `Blocked` with a condition naming it, and nothing is applied.

Next, prepare the [environment and secrets](./environments-and-secrets.md) the operator will resolve. Then choose either [an application image with a Performance](./performance.md) or [a published Stage application](./published-stage-applications.md).

## Values

Every value is documented in the chart's `values.yaml`. The ones most installs touch:

| Value | What it does |
| --- | --- |
| `image.tag` | The operator version. Defaults to the version of the chart |
| `managedNamespaces` | The namespaces the operator manages Performances in, and so the only namespaces where it may read and write Secrets and workloads. Defaults to the release namespace |
| `environments.azureKeyVault.workloadIdentity` | Gives the operator an Azure workload identity so it can read Azure Key Vault secrets |
| `environments.hashiCorpVault.kubernetesRole` | The role the operator logs in to HashiCorp Vault's Kubernetes auth method with |
| `environments.hashiCorpVault.tokenSecretName` | A Secret with a Vault token, for clusters without Kubernetes auth. Prefer Kubernetes auth |
| `resources`, `podSecurityContext`, `securityContext` | Resources and the security context; the defaults run as non-root with a read-only file system |

## What the operator may do

The chart's ClusterRole lets the operator do only this across the cluster:

- Performances and Productions: get, list, watch, update and patch, and their status.
- Custom resource definitions: get, only `certificates.cert-manager.io`, `issuers.cert-manager.io` and `perconaservermongodbs.psmdb.percona.com`, to tell whether cert-manager and the Percona operator are installed.
- Events: create and patch.

Everything else is granted by a Role in each namespace in `managedNamespaces`, and nowhere else:

- ConfigMaps: get.
- Secrets: get, create and update. It never lists, watches or deletes them.
- Services and Deployments: get, create and update. Ingresses: also delete, only for a Performance whose lifetime ended.
- StatefulSets: get, create and update, for Chronicle and the managed Neo4j of a Production.
- cert-manager Issuers and Certificates, and Percona `PerconaServerMongoDB` resources: get, create and update, for Productions.
- NetworkPolicies: get, create and update, for published Stage applications.
- Pod metrics (`metrics.k8s.io`): list, to report CPU and memory usage. Without metrics-server the usage is reported as not measured.

The operator deletes nothing but the Ingress of an expired Performance. Everything a Performance or Production creates carries an owner reference to it, so deleting it removes them; the volumes of a Production's managed database and graph are kept.
