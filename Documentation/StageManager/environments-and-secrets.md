---
title: Environments and secrets
description: How Stage Manager resolves a Cratis environment in the cluster and hands the values to containers as mounted files, never as environment variables.
---

A Performance names a Cratis environment and its file. The file is the one `cratis` keeps in `.cratis/environments/<name>.yaml`, supplied as a key of a ConfigMap (`<name>.yaml` unless `environment.configMapRef.key` says otherwise). Stage Manager resolves it with the same `Cratis.Environments` library the command line uses, so a value means the same thing locally and in the cluster.

| Secrets provider of the environment | How it is resolved in the cluster |
| --- | --- |
| Passphrase | The passphrase is read from the Secret named by `environment.passphraseSecretRef` (key `passphrase` unless set) |
| Azure Key Vault | Workload identity of the operator (`DefaultAzureCredential`). Set `environments.azureKeyVault.workloadIdentity` in the chart |
| HashiCorp Vault | Kubernetes auth with the operator's service account. Set `environments.hashiCorpVault.kubernetesRole` in the chart |

The Vault settings belong to the operator, not to a Performance: the library reads `VAULT_K8S_ROLE`, `VAULT_AUTH_MOUNT` and `VAULT_NAMESPACE` from the operator's environment, so every environment that uses HashiCorp Vault logs in with the same role. The address and mount come from the environment file's provider.

## Where a resolved value goes

A resolved value reaches a container in exactly one way: as a file in a mounted Secret volume.

- AuthProxy gets one file per environment key it needs, the key as the file name, in `/run/secrets/cratis`, and its `SecretsDirectory` points there.
- Ante gets one file per setting, named like the setting with `__` for `:`, in `/run/secrets/ante`, and `Ante:Onboarding:Secrets:Path` points there.
- The application gets the keys that belong to neither of them, in `/run/secrets/cratis`.
- The Chronicle connection Secret is projected into the volume as a file. It is not copied.

The Deployment spec holds structure only: provider kinds and identities, store names, paths. A value from the environment is never an environment variable, never in a ConfigMap, never in the status and never in the operator log. Pods roll when mounted content changes: the Deployment carries a keyed hash of the content as an annotation, which reveals nothing about a value.

An unresolved environment is reported in the `EnvironmentResolved` condition with the library's message, which names the key and the provider and never a value.
