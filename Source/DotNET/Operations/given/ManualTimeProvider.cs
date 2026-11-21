// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// A clock the specifications drive. Time moves only when a specification advances it, or when a short timer fires:
/// a timer due within <see cref="AutoFireWithin"/> (a pause between attempts) fires at once and moves the clock by its due
/// time, so nothing sleeps; a longer one (a time budget) fires only when the clock is advanced past it.
/// </summary>
public sealed class ManualTimeProvider : TimeProvider
{
    readonly List<ManualTimer> _timers = [];

    /// <summary>
    /// Gets or sets the due time up to which a timer fires immediately.
    /// </summary>
    public TimeSpan AutoFireWithin { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets the current time.
    /// </summary>
    public DateTimeOffset Now { get; private set; } = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Moves the clock and fires the timers that are now due.
    /// </summary>
    /// <param name="by">How far to move it.</param>
    public void Advance(TimeSpan by)
    {
        ManualTimer[] due;
        lock (_timers)
        {
            Now += by;
            due = [.. _timers.Where(timer => timer.IsDue(Now))];
        }

        foreach (var timer in due)
        {
            timer.Fire();
        }
    }

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow() => Now;

    /// <inheritdoc/>
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(callback, state, this, dueTime);
        lock (_timers)
        {
            _timers.Add(timer);
        }

        if (dueTime != Timeout.InfiniteTimeSpan && dueTime <= AutoFireWithin)
        {
            Now += dueTime;
            ThreadPool.QueueUserWorkItem(_ => timer.Fire());
        }

        return timer;
    }

    sealed class ManualTimer(TimerCallback callback, object? state, ManualTimeProvider owner, TimeSpan initialDue) : ITimer
    {
        DateTimeOffset? _due = initialDue == Timeout.InfiniteTimeSpan ? null : owner.Now + initialDue;

        public bool IsDue(DateTimeOffset now) => _due is { } due && due <= now;

        public void Fire()
        {
            _due = null;
            callback(state);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            _due = dueTime == Timeout.InfiniteTimeSpan ? null : owner.Now + dueTime;
            return true;
        }

        public void Dispose() => Remove();

        public ValueTask DisposeAsync()
        {
            Remove();
            return ValueTask.CompletedTask;
        }

        void Remove()
        {
            lock (owner._timers)
            {
                owner._timers.Remove(this);
            }
        }
    }
}
#endif
