// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Notes.Collecting.Publishing;

/// <summary>
/// Carries the sending application's identity from a note it sent to the replies the collector publishes for it.
/// </summary>
/// <remarks>
/// An application names itself on a note with <see cref="AudienceTags.From"/>. <see cref="Inbound"/> re-stamps it on the
/// private fact recorded for that note, so the collector's log remembers who sent it. <see cref="Resolve"/> later finds the
/// sender on the reply's trigger, else on the earliest tagged event of the same note, and <see cref="Address"/> opens the
/// Chronicle audience scope that addresses the outbox append to it. With no sender named the reply stays unaddressed.
/// </remarks>
public static class ReplyAudience
{
    static readonly IDisposable _none = new NoScope();

    /// <summary>
    /// Re-stamps the sender named by an inbound note on everything appended while the returned scope is open.
    /// </summary>
    /// <param name="inbound">The context of the inbound event.</param>
    /// <returns>The scope to dispose when the event has been recorded.</returns>
    public static IDisposable Inbound(EventContext inbound) =>
        SenderOf(inbound) is { } sender ? EventTags.Scope(AudienceTags.From, sender) : _none;

    /// <summary>
    /// Finds the audience a reply to the trigger is addressed to.
    /// </summary>
    /// <param name="eventStore">The event store holding the note's history.</param>
    /// <param name="trigger">The private event whose publication is the reply.</param>
    /// <returns>The audience, or <see langword="null"/> when the sender did not name itself.</returns>
    public static async Task<string?> Resolve(IEventStore eventStore, EventContext trigger)
    {
        var audience = SenderOf(trigger);
        if (audience is not null)
        {
            return audience;
        }

        var history = await eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, trigger.EventSourceId);
        return history.Select(entry => SenderOf(entry.Context)).FirstOrDefault(sender => sender is not null);
    }

    /// <summary>
    /// Opens the scope that addresses outbox appends to an audience. Call it from the method that appends, not from an
    /// <see langword="async"/> helper: an ambient scope does not flow back out of an awaited method.
    /// </summary>
    /// <param name="audience">The resolved audience, if any.</param>
    /// <returns>The scope to dispose when the reply has been appended.</returns>
    public static IDisposable Address(string? audience) => audience is null ? _none : EventTags.Audience(audience);

    static string? SenderOf(EventContext context) =>
        context.NamedTags.FirstOrDefault(tag => string.Equals(tag.Name.Value, AudienceTags.From, StringComparison.Ordinal))?.Value;

    sealed class NoScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
