// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Pipeline;

internal static partial class WatchdogRunLogMessages
{
    [LoggerMessage(LogLevel.Information, "healthy: nothing found, or the re-check was clean")]
    internal static partial void Healthy(this ILogger<WatchdogRun> logger);

    [LoggerMessage(LogLevel.Warning, "findings: {Findings}, alertable: {Alertable}")]
    internal static partial void Findings(this ILogger<WatchdogRun> logger, int findings, int alertable);

    [LoggerMessage(LogLevel.Warning, "  {Text}")]
    internal static partial void Finding(this ILogger<WatchdogRun> logger, string text);

    [LoggerMessage(LogLevel.Information, "nothing outside its re-alert cooldown; no alert sent")]
    internal static partial void CoolingDown(this ILogger<WatchdogRun> logger);

    [LoggerMessage(LogLevel.Information, "alert sent via {Channels}")]
    internal static partial void AlertSent(this ILogger<WatchdogRun> logger, string channels);

    [LoggerMessage(LogLevel.Information, "resolved since the last run: {Count}")]
    internal static partial void Resolved(this ILogger<WatchdogRun> logger, int count);

    [LoggerMessage(LogLevel.Information, "re-checking {Count} finding(s) before alerting")]
    internal static partial void Rechecking(this ILogger<WatchdogRun> logger, int count);

    [LoggerMessage(LogLevel.Warning, "no time left to re-check after {Seconds}s; alerting on the first pass")]
    internal static partial void NoTimeToRecheck(this ILogger<WatchdogRun> logger, int seconds);

    [LoggerMessage(LogLevel.Information, "re-check was clean; first-pass findings resolved")]
    internal static partial void RecheckClean(this ILogger<WatchdogRun> logger);

    [LoggerMessage(LogLevel.Information, "external heartbeat: {Outcome}")]
    internal static partial void HeartbeatSent(this ILogger<WatchdogRun> logger, string outcome);

    [LoggerMessage(LogLevel.Warning, "a failure while reporting a failure was ignored: {Type}")]
    internal static partial void SecondaryFailure(this ILogger<WatchdogRun> logger, string type);

    [LoggerMessage(LogLevel.Error, "watchdog run failed: {Type}")]
    internal static partial void RunFailed(this ILogger<WatchdogRun> logger, string type);
}
