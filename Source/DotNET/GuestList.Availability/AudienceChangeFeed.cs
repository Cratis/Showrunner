// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability.Audiences;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// The feed of changes to the audiences the product holds. The watcher publishes into it when a read model changes, and the
/// availability engine subscribes through <see cref="GuestListAudienceSource.OnChanged"/>, so an open session sees a grant
/// or a withdrawal without waiting for a new request.
/// </summary>
/// <remarks>
/// It also remembers when the last change was received, which is what <see cref="Lag"/> and the lag metric report.
/// </remarks>
/// <param name="time">The <see cref="TimeProvider"/>.</param>
/// <param name="logger">The logger.</param>
public class AudienceChangeFeed(TimeProvider time, ILogger<AudienceChangeFeed> logger)
{
    readonly Lock _lock = new();
    readonly List<Func<AudienceChange, ValueTask>> _handlers = [];
    long _lastReceivedTicks;

    /// <summary>
    /// Gets when the last change was received, or <see langword="null"/> when none has been since the product started.
    /// </summary>
    public DateTimeOffset? LastReceived => Interlocked.Read(ref _lastReceivedTicks) is var ticks and > 0 ? new DateTimeOffset(ticks, TimeSpan.Zero) : null;

    /// <summary>
    /// Gets the time since the last change was received, or <see langword="null"/> when none has been since the product
    /// started. It grows while nothing arrives, whether the operator is quiet or the link is down.
    /// </summary>
    public TimeSpan? Lag => LastReceived is { } received ? time.GetUtcNow() - received : null;

    /// <summary>
    /// Subscribes to changes.
    /// </summary>
    /// <param name="handler">Called for every change.</param>
    /// <returns>A subscription; dispose it to stop receiving changes.</returns>
    public IDisposable Subscribe(Func<AudienceChange, ValueTask> handler)
    {
        lock (_lock)
        {
            _handlers.Add(handler);
        }

        return new Subscription(() =>
        {
            lock (_lock)
            {
                _handlers.Remove(handler);
            }
        });
    }

    /// <summary>
    /// Records that a change was received and tells every subscriber. A subscriber that fails does not keep the others from
    /// hearing about it.
    /// </summary>
    /// <param name="change">The change.</param>
    /// <returns>An awaitable task.</returns>
    public async ValueTask Publish(AudienceChange change)
    {
        Func<AudienceChange, ValueTask>[] handlers;
        lock (_lock)
        {
            Interlocked.Exchange(ref _lastReceivedTicks, time.GetUtcNow().UtcTicks);
            handlers = [.. _handlers];
        }

        foreach (var handler in handlers)
        {
            try
            {
                await handler(change);
            }
            catch (Exception exception)
            {
                logger.SubscriberFailed(exception, change.Level, change.Key);
            }
        }
    }

    sealed class Subscription(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
