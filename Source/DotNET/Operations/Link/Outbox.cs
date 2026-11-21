// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// The watchdog's outbox of facts for Direct: ordered by a monotonic sequence that is never reused, kept durably between passes
/// and removed only up to the watermark Direct reports. It is bounded: beyond <c>limit</c> entries the oldest are evicted -
/// heartbeats first, since Direct folds them into status - and Direct records the missing sequences as a gap.
/// </summary>
/// <param name="instanceName">The instance the entries are for.</param>
/// <param name="snapshot">The saved outbox.</param>
/// <param name="limit">The most entries kept.</param>
/// <param name="timeProvider">The clock.</param>
/// <param name="newId">Creates the identity of a fact.</param>
public sealed class Outbox(string instanceName, OutboxSnapshot snapshot, int limit, TimeProvider timeProvider, Func<Guid>? newId = null)
{
    readonly List<WatchdogEventEnvelope> _entries = [.. snapshot.Entries];
    readonly Func<Guid> _newId = newId ?? Guid.NewGuid;

    /// <summary>
    /// Gets a value indicating whether the outbox has asked Direct for its watermark.
    /// </summary>
    public bool Baselined { get; private set; } = snapshot.Baselined;

    /// <summary>
    /// Gets the highest sequence Direct has taken.
    /// </summary>
    public long Watermark { get; private set; } = snapshot.Watermark;

    /// <summary>
    /// Gets the last sequence handed out.
    /// </summary>
    public long LastAssigned { get; private set; } = snapshot.LastAssigned;

    /// <summary>
    /// Gets how many entries were evicted before Direct took them.
    /// </summary>
    public long Evicted { get; private set; } = snapshot.Evicted;

    /// <summary>
    /// Gets how many entries Direct has not taken.
    /// </summary>
    public int Depth => _entries.Count;

    /// <summary>
    /// Appends a fact.
    /// </summary>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <param name="payload">The payload.</param>
    /// <param name="causation">The fact that caused it, or empty.</param>
    /// <returns>The identity of the new fact.</returns>
    public Guid Enqueue<TPayload>(TPayload payload, Guid causation = default)
        where TPayload : notnull
    {
        var id = _newId();
        _entries.Add(WatchdogEventEnvelope.For(instanceName, ++LastAssigned, id, causation, timeProvider.GetUtcNow(), payload));
        Evict();
        return id;
    }

    /// <summary>
    /// Gets whether an entry of a type is waiting.
    /// </summary>
    /// <param name="type">One of <see cref="WatchdogEventTypes"/>.</param>
    /// <returns>True when one is waiting.</returns>
    public bool HasPending(string type) => _entries.Exists(entry => entry.Type == type);

    /// <summary>
    /// Gets the next entries to deliver, in sequence.
    /// </summary>
    /// <param name="maximum">The most entries.</param>
    /// <returns>The entries.</returns>
    public IReadOnlyList<WatchdogEventEnvelope> Pending(int maximum) => [.. _entries.OrderBy(entry => entry.Sequence).Take(maximum)];

    /// <summary>
    /// Takes Direct's watermark as the truth about what it holds. A watermark beyond the sequences handed out means this outbox
    /// lost its earlier state: what it holds is renumbered above it, so it is never mistaken for entries Direct already took.
    /// </summary>
    /// <param name="watermark">Direct's watermark.</param>
    public void Rebase(long watermark)
    {
        if (watermark > LastAssigned)
        {
            var next = watermark;
            for (var index = 0; index < _entries.Count; index++)
            {
                _entries[index] = _entries[index] with { Sequence = ++next };
            }

            LastAssigned = next;
            Watermark = watermark;
        }
        else
        {
            Acknowledge(watermark);
        }

        Baselined = true;
    }

    /// <summary>
    /// Drops every entry up to the watermark Direct reports.
    /// </summary>
    /// <param name="watermark">Direct's watermark.</param>
    public void Acknowledge(long watermark)
    {
        _entries.RemoveAll(entry => entry.Sequence <= watermark);
        Watermark = Math.Max(Watermark, watermark);
    }

    /// <summary>
    /// Gets the durable form.
    /// </summary>
    /// <returns>The snapshot.</returns>
    public OutboxSnapshot ToSnapshot() => new(LastAssigned, Watermark, Baselined, Evicted, [.. _entries]);

    void Evict()
    {
        while (_entries.Count > Math.Max(limit, 1))
        {
            var victim = _entries.FindIndex(entry => entry.Type == WatchdogEventTypes.Heartbeat);
            _entries.RemoveAt(victim < 0 ? 0 : victim);
            Evicted++;
        }
    }
}
