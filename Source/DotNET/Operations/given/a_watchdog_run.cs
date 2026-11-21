// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Pipeline;
using Cratis.Showrunner.Operations.State;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// A watchdog run on a manual clock, with an in-memory state, one recording channel, a recording heartbeat and whatever checks
/// the specification adds. Nothing sleeps and nothing touches a network.
/// </summary>
public abstract class a_watchdog_run : Specification
{
    protected ManualTimeProvider _time;
    protected DateTimeOffset _started;
    protected InMemoryAlertState _state;
    protected RecordingChannel _channel;
    protected RecordingHeartbeat _heartbeat;
    protected FakeClusterReader _reader;
    protected List<IWatchdogCheck> _checks;
    protected List<IAlertChannel> _channels;
    protected WatchdogOptions _options;
    protected WatchdogOutcome _outcome;

    /// <summary>
    /// Gets the notification of the only delivery, for specifications that expect exactly one.
    /// </summary>
    protected AlertNotification Notification => _channel.Delivered.Single();

    void Establish()
    {
        _time = new ManualTimeProvider();
        _started = _time.Now;
        _state = new InMemoryAlertState();
        _channel = new RecordingChannel();
        _heartbeat = new RecordingHeartbeat();
        _reader = new FakeClusterReader();
        _checks = [];
        _channels = [_channel];
        _options = new WatchdogOptions { Namespace = "shop", RecheckPause = TimeSpan.FromSeconds(1) };
    }

    protected WatchdogRun CreateRun(bool withHeartbeat = true) => new(
        new FindingCollector(_checks, _options, _time),
        new WatchdogRunMonitor(_reader, _options),
        _state,
        new AlertDelivery(_channels, NullLogger<AlertDelivery>.Instance),
        withHeartbeat ? _heartbeat : null,
        _options,
        _time,
        NullLogger<WatchdogRun>.Instance);

    protected AlertStateSnapshot StateWith(string key, TimeSpan lastAlertedAgo, TimeSpan? firstSeenAgo = null) => AlertStateSnapshot.Empty with
    {
        LastAlerted = AlertStateSnapshot.Empty.LastAlerted.Add(key, _time.Now - lastAlertedAgo),
        FirstSeen = AlertStateSnapshot.Empty.FirstSeen.Add(key, _time.Now - (firstSeenAgo ?? lastAlertedAgo))
    };

    protected void FindsEveryPass(params Finding[] findings) => _checks.Add(StubCheck.Finding("check", findings));
}
#endif
