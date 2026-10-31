// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Acknowledging;

/// <summary>
/// Who in a tenant has acknowledged an announcement. It is the tenant's own: the same announcement is acknowledged
/// separately in every tenant's namespace.
/// </summary>
/// <param name="Id">The announcement.</param>
/// <param name="AcknowledgedBy">The people who acknowledged it.</param>
public record AnnouncementAcknowledgements(AnnouncementId Id, IEnumerable<UserSubject> AcknowledgedBy)
{
    /// <summary>
    /// Gets whether a person acknowledged the announcement.
    /// </summary>
    /// <param name="person">The person.</param>
    /// <returns><see langword="true"/> when they did; otherwise <see langword="false"/>.</returns>
    public bool WasAcknowledgedBy(UserSubject person) => AcknowledgedBy.Contains(person);

    /// <summary>
    /// Records that a person acknowledged the announcement, unless they already did.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static AnnouncementAcknowledgements Reduce(AnnouncementAcknowledged @event, AnnouncementAcknowledgements? current)
    {
        if (current is null)
        {
            return new(@event.AnnouncementId, [@event.UserSubject]);
        }

        return current.WasAcknowledgedBy(@event.UserSubject)
            ? current
            : current with { AcknowledgedBy = [.. current.AcknowledgedBy, @event.UserSubject] };
    }
}
