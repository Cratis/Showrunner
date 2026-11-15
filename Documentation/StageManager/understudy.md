---
title: Back up a Production with an Understudy
description: Schedule Chronicle database and key backups of a Production to object storage with an Understudy resource, and read their manifests, retention and status.
---

An `Understudy` backs up one [Production](./production.md). The operator turns it into Kubernetes CronJobs that dump the Production's databases to object storage, write a manifest beside every run, and apply retention. Each CronJob is owned by the Understudy, so deleting the Understudy removes its schedules. Backups that already exist in the bucket are never deleted by that.

## Create an Understudy

```yaml
apiVersion: showrunner.cratis.io/v1alpha1
kind: Understudy
metadata:
  name: chronicle
  namespace: shows
spec:
  productionRef: chronicle
  schedule: "0 * * * *"
  maxAgeDays: 35
  retention:
    hourly: 24
    daily: 7
    weekly: 4
  storage:
    provider: S3
    endpoint: https://s3.example.com
    region: eu-north-1
    bucket: chronicle-backups
    credentialsSecretName: backup-credentials
```

| Field | Meaning |
| --- | --- |
| `productionRef` | The Production in the same namespace to back up |
| `schedule` | A cron expression. Defaults to hourly |
| `retention` | How many hourly, daily, weekly, monthly and yearly backups to keep. Defaults to 24 hourly, 7 daily and 4 weekly |
| `maxAgeDays` | The age after which a backup is removed whatever the tiers say. Defaults to 35 |
| `databasePrefix`, `databases` | Which databases belong to the Production. The operator discovers the Chronicle event stores, namespaces and read-model databases and reports databases it cannot attribute as unclaimed |
| `storage` | Where the backups go |
| `keys` | Optional: back up the Production's encryption keys, see below |
| `image`, `workSize` | The job image and the scratch space it dumps into. The image defaults to the operator's own Understudy image |

## Object storage

The storage provider is one of `S3` (any S3-compatible endpoint, with `pathStyle` for those that need it), `AzureBlob` (`blobServiceUrl`) or `Volume` (`volumeClaimName`, for a PersistentVolumeClaim). Credentials come from a Secret named by `credentialsSecretName` and are never written into the status, events or logs.

Every provider implements the same abstraction, `IObjectStorage`: put, open for read, list by prefix, delete, create a time-limited download link, inspect the bucket and try to enforce an expiry lifecycle on it. When the provider can enforce expiry itself, the `LifecycleEnforced` condition says so; otherwise the pruner enforces it.

## What a backup looks like

Each run writes its files under a layout keyed by Production, event store, namespace and time, and finishes by writing `manifest.json`. The manifest is written last and only when every file was written, so a run with a manifest is complete, and the manifests alone are enough to list every backup.

The manifest records the schema version, the Production, the event store and namespace (null at a higher level), when the run started, the Chronicle version the Production ran, the retention in force and the entries: their type, database, file and size.

## Retention

After each run the pruner keeps every backup that qualifies for a tier (hourly, daily, weekly, monthly, yearly) and removes the rest, together with anything older than `maxAgeDays`. Files without a manifest, left by an interrupted run, are counted as orphans and removed.

## Key backups

When `keys` is set, a second CronJob reads the Production's encryption keys from the Vault, encrypts them for the certificate in `certificateSecret` as CMS, and stores only the encrypted result, with its own schedule, `maxAgeDays` and `storage`. The Vault token is read from `vaultTokenSecret`. The key bucket must not be the same as the data bucket: the operator refuses a key backup that would put keys beside the data they protect (`KeyBackupsReady`).

## Status

`kubectl get understudy` shows the phase, the last backup and its size. The status carries these conditions:

| Condition | True when |
| --- | --- |
| `SpecValid` | The spec is complete and consistent |
| `ProductionFound` | The referenced Production exists |
| `StorageReady` | The bucket can be reached and written |
| `LifecycleEnforced` | Expiry is enforced by the storage itself |
| `Scheduled` | The CronJobs exist |
| `BackupsHealthy` | The latest runs succeeded and are recent enough |
| `KeyBackupsReady` | Key backups are configured safely, when `keys` is set |

The operator records Kubernetes events for the facts of a run (`UnderstudyBackupTaken`, `BackupFailed` and so on) and serves Prometheus metrics at `/metrics` on the port of `metrics.port` in the chart (`showrunner_understudy_*`). The metrics are labelled with the Production, the type of backup and the outcome, never with a tenant or a person.

## Events and contracts

The facts of backups are published as contracts in the `Cratis.Showrunner.Understudy.Contracts` package, with a `BackupId` per run, so other services can react without referencing the operator.

## Install requirements

The chart grants the operator the rights to manage CronJobs and list Jobs in the namespaces it manages. The backup job image, `cratis/showrunner-understudy-image`, contains `mongodump`; the chart's `understudy.image` value selects it.
