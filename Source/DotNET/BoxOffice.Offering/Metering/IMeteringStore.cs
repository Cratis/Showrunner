// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Keeps the metering buckets and what has been reported beside the event log. Running totals are not events.
/// </summary>
/// <remarks>
/// <para>
/// An implementation must be durable, because it is what makes a closed day be reported once across restarts, and it
/// must make <see cref="Add"/> apply an event at most once per meter: a meter's events are added in sequence order, so
/// an event whose sequence number is not beyond the last one added to the bucket has already been counted.
/// </para>
/// <para>
/// Only the instance that holds the store (<see cref="TryHold"/>) measures, so the events of a tenant are added by one
/// process at a time.
/// </para>
/// </remarks>
public interface IMeteringStore
{
    /// <summary>
    /// Takes or renews the right to measure. Only one holder has it at a time, until its lease runs out.
    /// </summary>
    /// <param name="holder">The identity of the instance asking.</param>
    /// <param name="lease">How long the right lasts without being renewed.</param>
    /// <param name="now">The current time.</param>
    /// <returns>Whether the holder has the right now.</returns>
    Task<bool> TryHold(string holder, TimeSpan lease, DateTimeOffset now);

    /// <summary>
    /// Gets the sequence number of the first event of a tenant's event log that has not been measured.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The sequence number to read from.</returns>
    Task<EventSequenceNumber> GetCursor(TenantId tenant);

    /// <summary>
    /// Records that every event of a tenant's event log before a sequence number has been measured.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="next">The sequence number of the first event that has not been measured.</param>
    /// <returns>An awaitable task.</returns>
    Task SetCursor(TenantId tenant, EventSequenceNumber next);

    /// <summary>
    /// Gets the buckets of a tenant for a day.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="day">The day.</param>
    /// <returns>The buckets, or <see langword="null"/> when nothing was measured that day.</returns>
    Task<MeteringDay?> Get(TenantId tenant, ComputeDay day);

    /// <summary>
    /// Adds a quantity to a bucket, once per event.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="meter">The meter.</param>
    /// <param name="reading">The reading to add.</param>
    /// <param name="sequenceNumber">The sequence number of the event the reading was measured from.</param>
    /// <param name="dueForCorrection">Whether the day was already reported, so the addition has to be corrected.</param>
    /// <returns>Whether the quantity was added; <see langword="false"/> when the event had been added before.</returns>
    Task<bool> Add(TenantId tenant, MeterId meter, MeterReading reading, EventSequenceNumber sequenceNumber, bool dueForCorrection);

    /// <summary>
    /// Gets the days of a tenant that are not yet reported as closed, up to and including a day.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="through">The last day to include.</param>
    /// <returns>The open days, oldest first.</returns>
    Task<IReadOnlyList<MeteringDay>> GetOpen(TenantId tenant, ComputeDay through);

    /// <summary>
    /// Records that a day has been reported as closed.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="day">The day.</param>
    /// <returns>An awaitable task.</returns>
    Task Close(TenantId tenant, ComputeDay day);

    /// <summary>
    /// Gets the closed days of a tenant that have usage that is not yet reported as a correction.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The days awaiting a correction, oldest first.</returns>
    Task<IReadOnlyList<MeteringDay>> GetAwaitingCorrection(TenantId tenant);

    /// <summary>
    /// Records that a correction has been reported, unless more usage arrived after the one reported.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="day">The day.</param>
    /// <param name="due">The <see cref="MeteringDay.CorrectionDue"/> that was reported.</param>
    /// <returns>An awaitable task.</returns>
    Task Corrected(TenantId tenant, ComputeDay day, EventSequenceNumber due);
}
