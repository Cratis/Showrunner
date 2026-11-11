// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// What the event log says about one email: whether it is queued, how often it failed, when to try next and whether it is settled.
/// </summary>
/// <param name="Queued">The queued fact, when the email was queued.</param>
/// <param name="Failures">The highest failed attempt.</param>
/// <param name="NextAttemptAt">When the last failure said to try again.</param>
/// <param name="Settled">Whether the email was sent or failed for good.</param>
sealed record EmailHistory(EmailQueued? Queued, int Failures, DateTimeOffset? NextAttemptAt, bool Settled)
{
    /// <summary>
    /// Reads the history of one email from the event log.
    /// </summary>
    /// <param name="store">The event store the email is queued in.</param>
    /// <param name="about">Whatever the email is about.</param>
    /// <param name="id">The email.</param>
    /// <returns>The <see cref="EmailHistory"/>.</returns>
    public static async Task<EmailHistory> Read(IEventStore store, EventSourceId about, EmailId id)
    {
        var appended = await store.EventLog.GetForEventSourceIdAndEventTypes(
            about,
            [
                store.EventTypes.GetEventTypeFor(typeof(EmailQueued)),
                store.EventTypes.GetEventTypeFor(typeof(EmailAttemptFailed)),
                store.EventTypes.GetEventTypeFor(typeof(EmailSent)),
                store.EventTypes.GetEventTypeFor(typeof(EmailFailed))
            ]);

        return Fold(appended.Select(item => item.Content), id);
    }

    static EmailHistory Fold(IEnumerable<object> contents, EmailId id)
    {
        EmailQueued? queued = null;
        var failures = 0;
        DateTimeOffset? next = null;
        var settled = false;
        foreach (var content in contents)
        {
            switch (content)
            {
                case EmailQueued found when found.EmailId == id:
                    queued = found;
                    break;
                case EmailAttemptFailed failed when failed.EmailId == id:
                    failures = Math.Max(failures, failed.Attempt);
                    next = failed.NextAttemptAt;
                    break;
                case EmailSent sent when sent.EmailId == id:
                case EmailFailed failedForGood when failedForGood.EmailId == id:
                    settled = true;
                    break;
            }
        }

        return new(queued, failures, next, settled);
    }
}
