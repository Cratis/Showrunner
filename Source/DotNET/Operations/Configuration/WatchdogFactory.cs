// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Channels;
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;
using Cratis.Showrunner.Operations.Probe;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// Puts a watchdog together from <see cref="WatchdogSettings"/>: the enabled checks, the configured channels, the heartbeat and
/// the ConfigMap-backed state. The cluster and the HTTP client are passed in, so the same composition runs against fakes.
/// </summary>
/// <param name="settings">The settings.</param>
/// <param name="reader">The cluster reader.</param>
/// <param name="configMaps">The ConfigMap store the state is kept in.</param>
/// <param name="http">The HTTP client every probe and channel uses; its timeout bounds each call.</param>
/// <param name="emails">The email sender, used when email is configured.</param>
/// <param name="timeProvider">The clock.</param>
/// <param name="loggers">The logger factory.</param>
/// <param name="chronicleHttp">Creates the HTTP client that reads Chronicle, which may have to trust its certificate authority; <paramref name="http"/> is used when it is not given.</param>
/// <param name="runnerCluster">The runner scale set resources, used by the runners job.</param>
/// <param name="agentCluster">The agent worker Job operations, used by the agent-harness job.</param>
/// <param name="volumeHandles">The PersistentVolume volume handles, used by the volume sweep.</param>
/// <param name="gitHubHttp">The HTTP client that reads GitHub. It must not follow redirects, so the token never reaches another host; <paramref name="http"/> is used when it is not given.</param>
/// <param name="probeHttp">The HTTP client the probe calls the public endpoints with. It must not follow redirects, because a redirect is an answer and not a pass; <paramref name="http"/> is used when it is not given.</param>
public sealed class WatchdogFactory(
    WatchdogSettings settings,
    IClusterReader reader,
    IConfigMapStore configMaps,
    HttpClient http,
    IEmailSender? emails,
    TimeProvider timeProvider,
    ILoggerFactory loggers,
    Func<ChronicleSettings, HttpClient>? chronicleHttp = null,
    IRunnerCluster? runnerCluster = null,
    IAgentHarnessCluster? agentCluster = null,
    IVolumeHandles? volumeHandles = null,
    HttpClient? gitHubHttp = null,
    HttpClient? probeHttp = null)
{
    /// <summary>
    /// The checks that are jobs of their own: each runs in its own CronJob with its own alert state, so one job never resolves the
    /// findings of another, and the <c>cluster</c> run leaves them out.
    /// </summary>
    public static readonly IReadOnlyList<string> JobChecks = [RunnerPoolCheck.CheckName, AgentHarnessCheck.CheckName, VolumeSweepCheck.CheckName, ProbeCheck.CheckName];

    /// <summary>
    /// Creates the run of a job that has a check of its own: <c>runners</c>, <c>agent-harness</c>, <c>volume-sweeper</c> or <c>probe</c>.
    /// Its findings go through the same alert pipeline and channels as the cluster run, with state kept in a ConfigMap of its own.
    /// </summary>
    /// <param name="job">The name of the job, which is the name of its check.</param>
    /// <returns>The run.</returns>
    /// <exception cref="WatchdogConfigurationException">The job is not configured.</exception>
    public WatchdogRun CreateJobRun(string job) => new(
        new FindingCollector(Create(job) is { } check ? [check] : throw new WatchdogConfigurationException([$"the '{job}' job is not configured"]), settings.Options, timeProvider),
        new WatchdogRunMonitor(reader, settings.Options),
        new ConfigMapAlertState(configMaps, $"{settings.StateConfigMap}-{job}"),
        new AlertDelivery(CreateChannels(), loggers.CreateLogger<AlertDelivery>()),
        settings.Heartbeat is null ? null : new HealthchecksHeartbeat(http, settings.Heartbeat),
        settings.Options,
        timeProvider,
        loggers.CreateLogger<WatchdogRun>());

    /// <summary>
    /// Creates the external probe: the same probe, with the same channels, as one run from CI instead of from the cluster.
    /// </summary>
    /// <param name="log">Where the one-line results go.</param>
    /// <returns>The probe, the targets and the pause between the rounds.</returns>
    /// <exception cref="WatchdogConfigurationException">No probe targets are configured.</exception>
    public (ProbeRunner Runner, IReadOnlyList<ProbeTarget> Targets, TimeSpan RoundGap) CreateProbeRunner(Action<string> log)
    {
        var probe = settings.Probe ?? throw new WatchdogConfigurationException(["the probe needs PROBE_TARGETS"]);
        var runner = new ProbeRunner(
            new ProbeEvaluator(probeHttp ?? http, timeProvider),
            new AlertDelivery(CreateChannels(), loggers.CreateLogger<AlertDelivery>()),
            probe.PingUrl is null ? null : new HealthchecksProbeSink(http, probe.PingUrl),
            probe.IssueRepository is not null && probe.IssueToken is not null ? new GitHubIssueProbeSink(gitHubHttp ?? http, probe.IssueRepository, probe.IssueToken) : null,
            settings.Options.Deployment ?? "cratis",
            log);
        return (runner, probe.Targets, probe.RoundGap);
    }

    /// <summary>
    /// Creates the cluster watchdog run.
    /// </summary>
    /// <returns>The run.</returns>
    public WatchdogRun CreateClusterRun() => new(
        new FindingCollector(CreateChecks(), settings.Options, timeProvider),
        new WatchdogRunMonitor(reader, settings.Options),
        new ConfigMapAlertState(configMaps, settings.StateConfigMap),
        new AlertDelivery(CreateChannels(), loggers.CreateLogger<AlertDelivery>()),
        settings.Heartbeat is null ? null : new HealthchecksHeartbeat(http, settings.Heartbeat),
        settings.Options,
        timeProvider,
        loggers.CreateLogger<WatchdogRun>());

    /// <summary>
    /// Creates the run of the delivery test: the same channels as a real run and nothing else.
    /// </summary>
    /// <returns>The run.</returns>
    public WatchdogRun CreateTestAlertRun() => new(
        new FindingCollector([], settings.Options, timeProvider),
        new WatchdogRunMonitor(reader, settings.Options),
        new InMemoryAlertState(),
        new AlertDelivery(CreateChannels(), loggers.CreateLogger<AlertDelivery>()),
        null,
        settings.Options,
        timeProvider,
        loggers.CreateLogger<WatchdogRun>());

    /// <summary>
    /// Creates the channels that are configured.
    /// </summary>
    /// <returns>The channels.</returns>
    public IReadOnlyList<IAlertChannel> CreateChannels()
    {
        var channels = new List<IAlertChannel>();
        if (settings.DiscordWebhook is { } webhook)
        {
            channels.Add(new DiscordAlertChannel(http, webhook));
        }

        if (settings.Smtp is { } smtp && emails is not null)
        {
            channels.Add(new SmtpAlertChannel(emails, smtp));
        }

        if (settings.Direct is { } direct)
        {
            channels.Add(new DirectAlertChannel(http, direct));
        }

        return channels;
    }

    /// <summary>
    /// Creates the checks that are enabled.
    /// </summary>
    /// <returns>The checks, in the order they run.</returns>
    public IReadOnlyList<IWatchdogCheck> CreateChecks()
    {
        var checks = new List<IWatchdogCheck>();
        foreach (var name in settings.EnabledChecks.Where(name => !JobChecks.Contains(name)))
        {
            if (Create(name) is { } check)
            {
                checks.Add(check);
            }
        }

        return checks;
    }

    IWatchdogCheck? Create(string name) => name switch
    {
        PodsCheck.CheckName => new PodsCheck(reader),
        NodesCheck.CheckName => new NodesCheck(reader),
        VolumesCheck.CheckName => new VolumesCheck(reader, settings.Volumes),
        CertificatesCheck.CheckName => new CertificatesCheck(reader),
        CronJobFreshnessCheck.CheckName => new CronJobFreshnessCheck(reader, settings.CronJobs),
        PbmBackupsCheck.CheckName when settings.Pbm is { } pbm => new PbmBackupsCheck(reader, pbm.Namespace, pbm.MaximumAge),
        RestoreDrillCheck.CheckName when settings.RestoreDrill is { } drill => new RestoreDrillCheck(reader, drill.Namespace, drill.CronJob, drill.StaleAfter),
        ReadinessCheck.CheckName when settings.Readiness is { } readiness => new ReadinessCheck(reader, http, readiness, timeProvider),
        PublicUrlsCheck.CheckName => new PublicUrlsCheck(http, settings.PublicUrls, timeProvider),
        OperationalMetricsCheck.CheckName when settings.Prometheus is { } prometheus => new OperationalMetricsCheck(reader, new HttpPrometheusReader(http, prometheus), settings.Metrics),
        ChronicleObserversCheck.CheckName when settings.Chronicle is { } chronicle => new ChronicleObserversCheck(
            new LazyChronicleReader(() => new HttpChronicleReader(chronicleHttp?.Invoke(chronicle) ?? http, chronicle.Url, chronicle.ClientId, chronicle.ClientSecret)),
            settings.ChronicleObservers),
        VolumeSweepCheck.CheckName when settings.VolumeSweeper is { } sweeper && volumeHandles is not null => new VolumeSweepCheck(
            new UpCloudVolumeCloud(http, sweeper.Username, sweeper.Password, sweeper.Options.ClusterLabel),
            volumeHandles,
            sweeper.Options),
        AgentHarnessCheck.CheckName when settings.AgentHarness is { } harness && agentCluster is not null => new AgentHarnessCheck(agentCluster, harness),
        RunnerPoolCheck.CheckName when settings.Runners is { } pool && runnerCluster is not null => new RunnerPoolCheck(runnerCluster, configMaps, pool),
        ProbeCheck.CheckName when settings.Probe is { } probe => new ProbeCheck(new ProbeEvaluator(probeHttp ?? http, timeProvider), probe.Targets, probe.RoundGap),
        ActionsScheduleAuditCheck.CheckName when settings.GitHubAuditToken is { } token => new ActionsScheduleAuditCheck(
            new HttpGitHubReader(gitHubHttp ?? http, token),
            settings.ActionsAudit),
        TelemetryBudgetCheck.CheckName when settings.Prometheus is { } budgetPrometheus => new TelemetryBudgetCheck(new HttpPrometheusReader(http, budgetPrometheus), settings.TelemetryBudget),
        _ => null
    };
}
