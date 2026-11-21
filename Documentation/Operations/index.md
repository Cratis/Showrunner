---
title: Operations
description: The Showrunner watchdog checks a Kubernetes deployment, confirms what it finds and delivers alerts to Discord, email, Healthchecks and Cratis Direct.
---

Use Operations to watch a Kubernetes deployment and be told when something is wrong. The watchdog is a job that looks at the cluster every few minutes, confirms what it finds, and tells people. It is written in C# and works for any deployment. Cratis Infrastructure's own settings are the reference configuration below, and nothing in the package names a Cratis host or namespace.

One pass does this:

1. Every enabled check reads the cluster and returns findings. A check that fails or runs out of time becomes a finding of its own and never hides the others.
2. If anything was found, the watchdog waits a few seconds and looks again. Only a completely clean second look silences the first. A blip does not page anyone.
3. Each finding has a stable key. The watchdog remembers when it first saw a key and when it last alerted on it. It alerts when a finding is new, again after the re-alert interval (6 hours), and again, marked escalated, once it has been open longer than the escalation threshold (24 hours).
4. One alert goes to every configured channel: Discord, email and Cratis Direct. One failing channel does not silence the rest. The run fails only when no channel delivered.
5. A Healthchecks.io ping tells a dead-man's switch outside the cluster that the run completed, or that it failed.

## Packages

| Package | What it is |
| --- | --- |
| `Cratis.Showrunner.Operations` | The library: the alert pipeline, the channels, the checks, the Kubernetes reader and the environment configuration |
| `Cratis.Showrunner.StageManager.Pulumi` | The `Watchdog` Pulumi component that deploys the job |
| `cratis/showrunner-watchdog-image` | The container image the CronJob runs (`watchdog cluster`, `watchdog test-alert`) |

## Deploy it with Pulumi

```csharp
using Cratis.Showrunner.StageManager.Pulumi.Operations;

_ = new Watchdog("watchdog", new WatchdogArgs
{
    Namespace = "operations",
    Deployment = "kelda-prod",
    Direct = new WatchdogDirectArgs
    {
        Url = "https://direct.example.com",
        Token = config.RequireSecret("directAlertsToken"),
        Source = "kelda",
    },
});
```

This declares a ServiceAccount, a ClusterRole with the minimum read rights of the checks that run, a Role for the watchdog's own namespace, a Secret for the channel credentials, and a CronJob that runs every five minutes with `concurrencyPolicy: Forbid`, a run deadline of 240 seconds and a locked-down container. It also declares a suspended CronJob named `watchdog-test-alert` that delivers a test alert on demand:

```bash
kubectl -n operations create job --from=cronjob/watchdog-test-alert test-alert
```

A test alert goes to every channel, changes no state, does not ping the heartbeat and does not claim that health checks ran. Unlike a real alert, it fails when any channel fails, because partial delivery is not verified delivery.

Credentials (the Discord webhook, the SMTP user and password, the Direct token, the heartbeat URL) are Pulumi secrets. They are written to one Secret and referenced from the CronJob, so they never appear in a manifest, a log or a finding.

## Checks

By default the watchdog runs `pods`, `nodes` and `volumes`, plus every check that has been configured. Name the checks explicitly with `Checks` (or `WATCHDOG_CHECKS`) to run only those. The cluster role is granted from the same list, so a check that is not run is a read that is not allowed.

| Check | What it reports | Configure with |
| --- | --- | --- |
| `pods` | Containers stuck in CrashLoopBackOff, ImagePullBackOff, ErrImagePull, CreateContainerConfigError or CreateContainerError; pods evicted since the previous run; pods Pending for more than 15 minutes | nothing |
| `nodes` | Nodes that are not Ready; Memory, Disk or PID pressure | nothing |
| `volumes` | Persistent volume claims above the usage threshold (at most 85 percent); bound claims mounted by a settled pod that report no usage; nodes whose kubelet summary cannot be read | `VolumeThresholdPercent`, `VolumeAdvice` |
| `certificates` | cert-manager Certificates that are not Ready, expire within 14 days or have an unreadable expiry | `WatchCertificates` |
| `cronjobs` | CronJobs, such as database backups, that have not succeeded within their maximum age | `CronJobs` |
| `pbm-backups` | Percona Backup for MongoDB with no ready backup in the allowed age, and why the newest failed backup failed | `Pbm` |
| `restore-drill` | A restore drill that has not succeeded within 30 hours, or whose newest finished Job failed | `RestoreDrill` |
| `readiness` | Pods of a workload that refuse or never answer their readiness endpoint, probed by pod IP | `Readiness` |
| `public-urls` | Public URLs that fail, tried three times each; a 4xx counts as the product answering | `PublicUrls` |

A finding that cannot be read is never treated as healthy: an unreadable source becomes its own finding, for example `pods:api`, so a missing permission is visible.

Errors that a Kubernetes custom resource or a transport carries can hold connection strings and credentials. They are redacted (URLs, authorization values and credential assignments) and truncated before they become part of a finding.

## Configuration

The job reads environment variables. The names the Python watchdog used are kept, so a deployment moves over without renaming. `WatchdogArgs` sets all of them; set them by hand only when you deploy the image another way.

| Variable | Meaning | Default |
| --- | --- | --- |
| `WATCHDOG_NAMESPACE` | The namespace the watchdog runs in; its state ConfigMap and its own Jobs live there | the pod's namespace |
| `WATCHDOG_DEPLOYMENT` | The name shown in alert subjects | the namespace |
| `WATCHDOG_CHECKS` | The checks to run, comma separated | the basic three plus every configured check |
| `WATCHDOG_STATE_CONFIGMAP` | The ConfigMap the cooldown state is kept in | `cluster-health-watchdog-state` |
| `WATCHDOG_SCHEDULE` | The CronJob schedule. In the form `*/N * * * *` the watchdog reports a gap of three intervals between runs | none |
| `WATCHDOG_CRONJOB` | The watchdog's own CronJob. A failed Job of it is reported once | none |
| `WATCHDOG_JOB_NAME` | The name of the current Job, so a crash is reported once for it | none |
| `REALERT_SECONDS` | How long an alerted finding stays quiet, such as `6h` or `21600` | `6h` |
| `ESCALATE_AFTER_SECONDS` | How long a finding may stay open before it is escalated | `24h` |
| `DEADLINE_SECONDS` | How long a run may take; the Job's deadline carries the same number | `240` |
| `DISCORD_WEBHOOK_URL` | The Discord webhook | none |
| `SMTP_SERVER`, `SMTP_PORT`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `ALERT_FROM_NAME`, `ALERT_FROM`, `ALERT_TO` | The email channel, with STARTTLS. All seven are required when `SMTP_SERVER` is set | none |
| `DIRECT_ALERTS_URL`, `DIRECT_ALERTS_TOKEN`, `DIRECT_ALERTS_SOURCE` | Cratis Direct: its base URL, the bearer token of the alert source, and the optional source name | none |
| `WATCHDOG_HEARTBEAT_URL` | The Healthchecks.io check URL | none |
| `WATCHDOG_CRONJOBS` | `[namespace/]name=maxAge[?]`, comma separated. A trailing `?` says the CronJob may be missing | none |
| `PBM_NAMESPACE`, `PBM_MAX_AGE` | The Percona backups to watch and how old the newest may be | none, `3h` |
| `RESTORE_DRILL_NAMESPACE`, `RESTORE_DRILL_CRONJOB`, `RESTORE_DRILL_STALE` | The restore drill to watch | none, `restore-drill`, `30h` |
| `READINESS_NAMESPACE` (or `CORE_NAMESPACE`), `READINESS_SELECTOR`, `READINESS_CONTAINER`, `READINESS_PORT`, `READINESS_PATH`, `READINESS_HANG_ADVICE` | The workload to probe | none, `app=core`, any, `8080`, `/health/ready` |
| `PUBLIC_URLS` | Public URLs to probe, comma separated | none |
| `VOLUME_THRESHOLD_PERCENT`, `VOLUME_ADVICE` | The volume threshold and what the finding tells the reader to do | `85` |

A wrong value stops the run before it looks at anything, and the error names the variable, never its value.

The state ConfigMap keeps the format the Python watchdog used: each alerted finding's key maps to the epoch seconds it was last alerted, next to `__last_check`, `__first_seen`, `__summary_streaks`, `__reported_failed_jobs` and `__actions_audit`. A deployment that moves to this watchdog keeps its cooldowns.

## Alerts in Cratis Direct

Register an alert source in Direct, give it a token, and pass the token as `Direct.Token`. The watchdog posts to `POST {url}/webhooks/alerts` with `Authorization: Bearer <token>` and, for each finding that is new, due again or escalated:

```json
{
  "title": "[kelda-prod] pod:shop/web:CrashLoopBackOff",
  "summary": "shop/web container 'web': CrashLoopBackOff (restarts: 7) (open since 2026-07-01 12:00 UTC, 0h)",
  "severity": "critical",
  "fingerprint": "pod:shop/web:CrashLoopBackOff",
  "labels": { "deployment": "kelda-prod" },
  "source": "kelda"
}
```

The finding's key is the fingerprint, so a repeat is the same alert in Direct. A finding that has been open past the escalation threshold is pushed as `critical`, which Direct records as a change of severity. Direct's webhook only raises alerts. It has no way to close one, so a resolved finding is not pushed: the alert stays open in Direct until someone resolves it there, and a recurrence raises it again. A test alert uses the fingerprint `watchdog:test-alert` and severity `information`.

## Reference configuration: Cratis Infrastructure

```csharp
_ = new Watchdog("cluster-health-watchdog", new WatchdogArgs
{
    Namespace = "operations",
    Deployment = "cratis",
    WatchCertificates = true,
    Discord = new WatchdogDiscordArgs { WebhookUrl = config.RequireSecret("discordWebhook") },
    Smtp = new WatchdogSmtpArgs { /* server, credentials, from and to */ },
    Direct = new WatchdogDirectArgs { Url = "https://direct.example.com", Token = config.RequireSecret("directAlertsToken") },
    HeartbeatUrl = config.RequireSecret("healthchecksUrl"),
    CronJobs =
    [
        new WatchdogCronJobArgs { Namespace = "databases", Name = "db-backups", MaximumAge = "3h" },
        new WatchdogCronJobArgs { Namespace = "databases", Name = "db-backups-offsite", MaximumAge = "26h" },
        new WatchdogCronJobArgs { Name = "runner-watchdog", MaximumAge = "30m", Optional = true },
    ],
    Pbm = new WatchdogPbmArgs { Namespace = "databases" },
    RestoreDrill = new WatchdogRestoreDrillArgs { Namespace = "databases" },
    Readiness = new WatchdogReadinessArgs { Namespace = "core", Selector = "app=core", Container = "core" },
    PublicUrls = ["https://app.example.com/"],
});
```

Replace the hosts, namespaces and ages with your own. A deployment that is smaller, such as Kelda Kraft, runs only what makes sense for it: `Checks = ["pods", "nodes"]` and a Direct channel is a complete watchdog.

## Not yet in the C# watchdog

The Python watchdog still owns these until they are ported: Prometheus operational metrics (node filesystem usage, platform targets), Chronicle observers and failed partitions, the telemetry budget, the GitHub Actions schedule audit, and the runner, agent harness and volume sweeper watchdogs.
