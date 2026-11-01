---
title: Install the operator
description: Install the Stage Manager operator with the cratis/showrunner-operator Helm chart, give it an identity for your secrets providers and see what it may do in the cluster.
---

The operator is published as a container image, `cratis/showrunner-operator`, and as a Helm chart of the same version, so a chart always installs the operator it was released with.

```shell
helm install stage-manager oci://registry-1.docker.io/cratis/showrunner-operator \
  --namespace stage-manager --create-namespace
```

The chart installs the custom resource definitions from its `crds` folder, the operator Deployment and its permissions. Helm installs CRDs once and does not upgrade them, so apply the `crds` folder of a new chart yourself when you upgrade.

## Values

Every value is documented in the chart's `values.yaml`. The ones most installs touch:

| Value | What it does |
| --- | --- |
| `image.tag` | The operator version. Defaults to the version of the chart |
| `environments.azureKeyVault.workloadIdentity` | Gives the operator an Azure workload identity so it can read Azure Key Vault secrets |
| `environments.hashiCorpVault.kubernetesRole` | The role the operator logs in to HashiCorp Vault's Kubernetes auth method with |
| `environments.hashiCorpVault.tokenSecretName` | A Secret with a Vault token, for clusters without Kubernetes auth. Prefer Kubernetes auth |
| `resources`, `podSecurityContext`, `securityContext` | Resources and the security context; the defaults run as non-root with a read-only file system |

## What the operator may do

The chart's ClusterRole is the whole list, and it is short:

- Performances: get, list, watch, update and patch, and their status.
- Productions: get, to read the connection Secret a Production reports.
- ConfigMaps: get.
- Secrets: get, create and update. It never lists or watches Secrets.
- Services, Deployments and Ingresses: get, create and update.
- Events: create and patch.

The operator never deletes anything directly. Everything a Performance creates carries an owner reference to it, so deleting the Performance removes it all.

## Managed MongoDB

The Percona Operator for MongoDB is meant to be an optional dependency of the chart for managed MongoDB. That work has not landed, so the chart does not declare the dependency yet.
