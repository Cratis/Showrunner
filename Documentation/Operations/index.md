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
| `cratis/showrunner-watchdog-image` | The container image the CronJobs run (`watchdog cluster`, `test-alert`, `runners`, `agent-harness`, `volumes`, `probe`, `probe-external`) |

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

## Linking the watchdog to Cratis Direct

The alert webhook above is the light way to tell Direct about a problem. The **Direct link** makes the watchdog a real instance of Direct's watchdog protocol (`/webhooks/watchdogs/{name}/...`): Direct can enroll it, give it signed configuration, receive its facts reliably and govern the fixes it may run.

| | Alert webhook (`Direct`) | Direct link (`DirectLink`) |
| --- | --- | --- |
| What it is | One bearer token, `POST /webhooks/alerts` per alert | Enrollment, claim, signed configuration, outbox, remedies |
| State | None | Credential, held configuration, durable outbox, in the cluster |
| Direct can add checks | No | Yes: the endpoint checks defined for the instance run on top of the local ones |
| A resolved problem | Not reported (Direct's webhook cannot close an alert) | `check-recovered` closes it |
| Delivery while Direct is down | The alert is lost for that channel | Facts are queued durably and delivered in order afterwards |
| Fixes | No | Governed remedies (below) |
| Use it when | You want alerts in Direct and nothing else | You want Direct to own the watchdog's configuration, see its health and govern fixes |

They are independent and can be used together: an alert is then pushed by the webhook and reported as a fact by the link.

### Enroll in Direct, step by step

1. In Direct, enroll a watchdog instance (the operations area, *Watchdogs*) and choose its name, for example `showrunner-prod`. Direct shows the one-time enrollment token once.
2. Define the instance's checks and remedies in Direct if you want any, and set the offline policy (observe and queue is the default).
3. Add `DirectLink` to the `Watchdog` component, with the token as a Pulumi secret:

```csharp
var stageManager = new StageManagerOperator("stage-manager", new StageManagerOperatorArgs { Version = "2.17.1", ManagedNamespaces = ["shop"] });

_ = new Watchdog("watchdog", new WatchdogArgs
{
    Namespace = "operations",
    Image = stageManager.WatchdogImage,
    DirectLink = new WatchdogDirectLinkArgs
    {
        Url = "https://direct.example.com",
        InstanceName = "showrunner-prod",
        EnrollmentToken = config.RequireSecret("directEnrollmentToken"),
        Remedies = [new WatchdogRemedyArgs { Remedy = "restart-web", Namespace = "shop", Deployment = "web" }],
    },
});
```

4. `pulumi up`. The next pass claims the instance with the token, stores the instance credential and the signing key in the Secret `<name>-direct-link`, applies and acknowledges the configuration, and reports. The job log ends with one status line:

```text
direct link: connected; epoch 1; revision 3; outbox 0 pending, watermark 42, 5 delivered, 0 evicted; 0 remedy action(s)
```

5. The token is no longer used. Remove `EnrollmentToken` from the stack when you like; the credential is in the cluster. To enroll again (a rotated token or key, a new cluster), delete the `<name>-direct-link` Secret and ConfigMap and supply a new token.

### What it does in a pass

- **Claim.** The first claim sends the token and receives the credential once; later claims send the credential and the same session, so every pass is one process to Direct. The fencing epoch comes back with each claim. If Direct answers `409`, another session holds the instance: the watchdog stops delivering and running remedies through the link, reports `Fenced`, and keeps alerting locally.
- **Configuration.** Direct's snapshot is signed with HMAC-SHA256 over a canonical form. The watchdog verifies it under the key it pinned at the first claim and applies it only when it is for this instance, newer (or a renewal of the revision held), unexpired and its digest matches. It asks for a fresh lease once past half of it, and acknowledges each applied revision (retried until Direct has it). Local environment configuration always applies; Direct's endpoint checks come on top of it. When Direct is unreachable the held configuration keeps working until its lease lapses, then only local configuration applies, and the status says so.
- **Facts.** Cluster findings and Direct's checks become `check-failed` (the first, second and then every twelfth consecutive failure; Direct raises an alert from the second) and `check-recovered`, plus remedy events and a heartbeat. They go into an outbox kept in the ConfigMap `<name>-direct-link`: ordered by a sequence that is never reused, flushed at the start and the end of every pass, removed only up to the watermark Direct reports, so a lost response only means a batch is sent again and Direct deduplicates. The outbox holds at most `OutboxLimit` entries (800); beyond that heartbeats and then the oldest are evicted, counted in the status, and Direct records the gap.
- **Remedies.** Direct decides *whether* a remedy runs; the watchdog re-checks and carries it out. While connected it requests a remedy once per failure episode (idempotent), and executes only an authorization Direct returns, after re-checking revision, epoch, expiry, that the check is still failing, and the attempt budget and cooldown. While Direct is unreachable the offline policy applies exactly as Direct defines it: nothing runs unless the signed, unexpired configuration grants `Automatic` offline mode, the outbox is durable, the claim was confirmed within its lease, the verification check exists and is failing on fresh evidence, and budget and cooldown allow it. `Review` mode never runs offline. A remedy without a local executor is denied, never guessed at.
- **Executors.** `IRemedy` is the extension point. The one that ships restarts a single named Deployment (a rolling restart through the pod template, as `kubectl rollout restart` does), bound by `Remedies` to the name of the remedy in Direct, with RBAC `get` and `patch` on exactly that Deployment. Nothing destructive ships. `AllowHttpRemedies` additionally lets a remedy with no binding execute the HTTP request its signed configuration pins; it is off by default because it makes the watchdog call whatever URL Direct's configuration names.
- **Status.** The status line above is logged by every pass and the same data is written to the `status` key of the link ConfigMap. It never contains a credential, key or secret.

### Security model

- The enrollment token and the instance credential are bearer secrets: the token is a Pulumi secret in the channels Secret; the credential, the pinned signing key, the held configuration (it carries the secrets of Direct's checks and remedies) and the session live in the Secret `<name>-direct-link`. The outbox, memory and status hold none of them (a spec checks it).
- The link's HTTP client does not follow redirects, so a bearer token is never sent to another host.
- The signing key is pinned. A different key later is refused, not trusted; recover by deleting the link Secret on purpose.
- If the saved state cannot be read, the link refuses to claim, because a first claim would discard the credential.
- RBAC is added only when `DirectLink` is set: get, patch and create on the link ConfigMap and Secret by name, and `get`/`patch` on the bound Deployments.
- Direct's `Operations.Contracts` is not published as a package, so the wire types are re-declared in `Cratis.Showrunner.Operations.Link.Protocol` with the same JSON shape and options. Conformance specs verify a configuration signed by Direct's own code, byte for byte, and the request and event shapes; their fixtures name the Direct commit they come from, and fail when the types drift.

### What it learned from Direct's own watchdog

Direct's watchdog is a long-running daemon. This one is a CronJob pass with no disk, so state a daemon keeps in memory or SQLite is loaded and saved around each pass. Adopted as they are: enrollment by one-time token, the signed configuration with leases and acknowledgement, the idempotent outbox with watermark receipts, governed remedies with the offline policy, status reporting. Adapted: the session id is persisted (so a new pod is not a clone), the journal is a bounded ConfigMap rather than SQLite, check intervals are bounded below by the CronJob cadence. Not taken: the local Chronicle kernel and replication of the full image, and the bridge.

### Environment variables

| Variable | Meaning |
| --- | --- |
| `DIRECT_LINK_URL`, `DIRECT_LINK_INSTANCE` | Base URL and instance name. Both or neither |
| `DIRECT_LINK_ENROLLMENT_TOKEN` | The one-time token (a Secret reference when deployed by the component) |
| `DIRECT_LINK_RESOURCE` | The Direct resource id observations are reported against; derived from the instance name when empty |
| `DIRECT_LINK_STATE_NAME` | Name of the ConfigMap and Secret the link keeps its state in |
| `DIRECT_LINK_REMEDIES` | `remedy=namespace/deployment` entries, comma separated |
| `DIRECT_LINK_HTTP_REMEDIES` | `true` to allow HTTP remedies |
| `DIRECT_LINK_OUTBOX_LIMIT` | Most outbox entries kept |

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

## More checks of the cluster pass

These run in the same pass as the checks above, are named in `WATCHDOG_CHECKS` and are turned on by what you give the component.

| Check | What it reports | Turned on by |
| --- | --- | --- |
| `volumes` | A volume above the threshold, a kubelet summary that cannot be read, a claim with no usage. A node that is leaving or has just joined (10 minutes, 30 for a batch node labeled `workload=batch`) is not read, and an unreadable summary of a batch node alerts only after three consecutive runs | `VolumeThresholdPercent`, `VolumeAdvice` |
| `metrics` | A node whose root filesystem is over 85%, a node, kube-state-metrics or collector scrape target that is down, and any of those that is missing from Prometheus. A source that cannot be read is a finding, never a pass | `Prometheus` |
| `telemetry-budget` | Telemetry refused, dropped or failed in the last 15 minutes (collector, delta accumulation, Loki, Tempo), oversized scrapes with the targets that rejected them, head series over the budget, any container above 90% of its memory limit, telemetry Services that are not scraped | `Prometheus` (and `SeriesThreshold`) |
| `chronicle-observers` | Partitions that ran out of retries, partitions still failing after 15 minutes, quarantined observers, in every namespace of every event store (Chronicle's System store is left out) | `Chronicle` |
| `actions-audit` | A GitHub Actions workflow that is disabled, or whose scheduled runs stopped for more than twice their inferred interval (at least an hour). GitHub is asked once a day and in between the findings are repeated; a rate limit backs off at least a day | `GitHubAudit` |

The Prometheus and Chronicle readers fail closed: an error status, a warning, a result that is not a vector or a sample that cannot be read is an exception, and the check turns that into a finding. Secrets (the Chronicle client secret, the GitHub token) are written to the channels Secret as Pulumi secrets and are never part of a finding, a log line or an error.

## Jobs of their own

Some watchdogs act, and some must not share the alert state of the cluster pass. Each runs as a CronJob of its own, from the same image, with a ServiceAccount, rights and Secret of its own, so that what one job may do is never what another may do. They deliver through the same channels, including Cratis Direct, with the same cooldown and escalation, and keep their state in `NAME-state-CHECK`.

| Command | What it does | Component input | Rights |
| --- | --- | --- | --- |
| `runners` | Stops a job that has held an ephemeral runner for the limit (90 minutes; 0 only reports) by deleting its runner, and replaces a runner with no job that is stuck pending (30 minutes). The time is measured from when the job was first seen, never from the pod's age. At most 3 runners per pass; more than that stops nothing and is a critical finding | `Runners` | `ephemeralrunners` list and delete in the runner namespace, and its own state ConfigMaps |
| `agent-harness` | Finds worker Jobs that can never start because a Secret their Pod mounts does not exist and, when `CleanupEnabled`, deletes them. Every Job and Pod must look untouched, the missing Secret must be confirmed by recent repeated `FailedMount` events, and the Job and its Pods are read again twice before a foreground delete with the Job's UID and resource version as preconditions. Missing evidence is never permission; at most 3 per pass | `AgentHarness` | jobs get, list and delete, pods, events and secrets read in the agent namespace. Only whether a Secret exists is asked, with a metadata request, and its data is never read |
| `volumes` | Detaches and deletes CSI volumes in the UpCloud account that no PersistentVolume refers to. Only volumes of this cluster (exactly one cluster id label among the volumes that back PersistentVolumes) older than 2 hours are candidates, more than 10 trips a fuse, PersistentVolumes are read again before acting, and an empty or ambiguous view is a critical finding instead of a pass | `VolumeSweeper` | `persistentvolumes` list. The cloud credentials are in a Secret of the job |
| `probe` | Calls the public endpoints the way an anonymous visitor does. A request that fails is asked again once after a pause, and only one that fails both rounds is a finding | `Probe` | none beyond its own state |
| `probe-external` | The same probe run from outside the cluster, which can see an ingress or load balancer that is down. It needs no cluster access, keeps no state, writes a Markdown summary (to `GITHUB_STEP_SUMMARY` when set), pings Healthchecks (`PROBE_PING_URL`, `/fail` with the failing endpoints when down), keeps one `probe-down` GitHub issue per outage (`PROBE_ISSUES_REPOSITORY` and `PROBE_ISSUES_TOKEN`) and alerts the shared channels. It is not declared by Pulumi: run the image from CI with `args: [probe-external]` and the environment variables below | CI | none |

A job that acts does so once per run: the confirmation look at the end of the pass replays what the job already did.

```csharp
_ = new Watchdog("watchdog", new WatchdogArgs
{
    Namespace = "operations",
    Direct = new WatchdogDirectArgs { Url = "https://direct.example.com", Token = config.RequireSecret("directAlertsToken") },
    Prometheus = new WatchdogPrometheusArgs { Url = "http://prometheus-server.infrastructure:80" },
    Chronicle = new WatchdogChronicleArgs { Url = "https://chronicle:8080", ClientId = "watchdog", ClientSecret = config.RequireSecret("chronicleSecret") },
    GitHubAudit = new WatchdogGitHubAuditArgs { Token = config.RequireSecret("githubAuditToken") },
    Runners = new WatchdogRunnersArgs { Namespace = "arc-runners", ScaleSets = ["big", "small"] },
    AgentHarness = new WatchdogAgentHarnessArgs { Namespace = "direct" },
    VolumeSweeper = new WatchdogVolumeSweeperArgs { Username = config.RequireSecret("upcloudUser"), Password = config.RequireSecret("upcloudPassword") },
    Probe = new WatchdogProbeArgs { Targets = [new WatchdogProbeTargetArgs { Name = "app", BaseUrl = "https://app.example.com", Checks = [new WatchdogProbeCheckArgs { Id = "health", Path = "/health", ExpectStatus = 200 }] }] },
});
```

### Environment variables of the executable

| Variable | Used by |
| --- | --- |
| `PROMETHEUS_URL`, `PLATFORM_NAMESPACE`, `PROMETHEUS_SERIES_THRESHOLD` | `metrics`, `telemetry-budget` |
| `CHRONICLE_URL`, `CHRONICLE_CLIENT_ID`, `CHRONICLE_CLIENT_SECRET`, `CHRONICLE_CA_CERTIFICATE` | `chronicle-observers` |
| `GITHUB_AUDIT_TOKEN`, `GITHUB_AUDIT_ORG` | `actions-audit` |
| `RUNNER_NAMESPACE`, `RUNNER_SCALE_SETS`, `MAX_JOB_MINUTES`, `PENDING_MINUTES`, `RUNNER_STATE_CONFIGMAP` | `runners` |
| `AGENT_NAMESPACE`, `CLEANUP_ENABLED` | `agent-harness` |
| `UPCLOUD_USERNAME`, `UPCLOUD_PASSWORD`, `MIN_AGE_SECONDS`, `MAX_PER_RUN`, `DRY_RUN` | `volumes` |
| `PROBE_TARGETS` (JSON: `name`, `baseUrl`, `checks[]` with `id`, `method`, `path`, `accept`, `body`, `expectStatus`, `timeoutMs`), `PROBE_BASE_URLS` (JSON overrides by target name), `PROBE_ROUND_GAP_SECONDS` (at most 30), `PROBE_PING_URL`, `PROBE_ISSUES_REPOSITORY`, `PROBE_ISSUES_TOKEN` | `probe`, `probe-external` |

## Migrating Cratis Infrastructure

Cratis Infrastructure maps its current Pulumi settings onto the component like this:

| Infrastructure setting | Watchdog input |
| --- | --- |
| `DiscordWebhookUrl` | `Discord.WebhookUrl` |
| `Mail` (server, credentials, from, to) | `Smtp` |
| the Cratis Direct alert source and token | `Direct.Url`, `Direct.Token`, `Direct.Source` |
| the Healthchecks check URL | `HeartbeatUrl` |
| the watchdog schedule, deadline, re-alert and escalation | `Schedule`, `DeadlineSeconds`, `RealertInterval`, `EscalateAfter` |
| the backup, off-site and runner-watchdog CronJobs | `CronJobs` |
| Percona backups, the restore drill, the core readiness probe, the public URLs | `Pbm`, `RestoreDrill`, `Readiness`, `PublicUrls` |
| the Prometheus URL, the telemetry budget series threshold | `Prometheus` |
| the Chronicle client | `Chronicle` |
| `GITHUB_AUDIT_TOKEN` | `GitHubAudit` |
| the runner watchdog arguments (namespace, scale sets, limits) | `Runners` |
| the agent harness watchdog arguments | `AgentHarness` (set `CleanupEnabled` last, once the report matches what you expect) |
| the UpCloud credentials of the volume sweeper | `VolumeSweeper` |
| `scripts/probe/targets.json` | `Probe.Targets` for the in-cluster probe, and `PROBE_TARGETS` for the CI job |

The state ConfigMap keeps its format, so cooldowns survive the move. Move one watchdog at a time: deploy the C# one with its channels, confirm a test alert, then remove the old one.

Once everything is moved, these Infrastructure files can be deleted: `Core/Services/ClusterHealthWatchdog.cs`, `RunnerWatchdog.cs`, `RunnerWatchdogScript.cs`, `AgentHarnessWatchdog.cs`, `AgentHarnessWatchdogArgs.cs`, `AgentHarnessWatchdogScript.cs`, `VolumeSweeper.cs`, `volume-sweeper.py`, the Python specs under `Core/Services/specs/` that test them (`test_cluster_health_watchdog.py`, `test_watchdog_robustness.py`, `test_runner_watchdog*.py`, `test_agent_harness_watchdog*.py`, `test_restore_drill_watchdog.py`, `test_actions_schedule_audit.py`, `test_prometheus_headroom.py` (its watchdog part), `test_chronicle_*` watchdog parts, `test_telemetry_budget.py` (its watchdog part)), and `scripts/probe/` with its CI workflow step, replaced by a step that runs `probe-external`.

## What stays outside the watchdog

These are Pulumi-side policy and declarations, not watchdog behavior, so they are not ported and stay in Infrastructure:

- `TelemetryBudget.cs` and `InvalidTelemetryBudget.cs` decide the budget the collector and Prometheus are *configured* with. The watchdog only reads whether that budget is being exceeded.
- The Prometheus relabel and drop rules, which `test_prometheus_headroom.py` checks (a Prometheus configuration, not a check).
- The restore drill itself (`Core/MongoDB/RestoreDrill*.cs`, `restore-drill.sh`) is a database job; the watchdog only checks that it keeps succeeding (`restore-drill`).
- `test_backup_volume_resize.py` asserts how backup volumes are sized, which is Pulumi-side declaration.
