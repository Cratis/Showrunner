// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Channels;
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;
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
public sealed class WatchdogFactory(
    WatchdogSettings settings,
    IClusterReader reader,
    IConfigMapStore configMaps,
    HttpClient http,
    IEmailSender? emails,
    TimeProvider timeProvider,
    ILoggerFactory loggers)
{
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
        foreach (var name in settings.EnabledChecks)
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
        _ => null
    };
}
