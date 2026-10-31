// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing;

/// <summary>
/// An announcement the product holds for its tenants: received and not retracted. It lives in the product's own
/// namespace and is the same for every tenant; which tenants it reaches is its <see cref="Audience"/>.
/// </summary>
/// <remarks>
/// <para>
/// It is a self-reducing read model. A retraction removes it. Nothing here filters by date: an announcement scheduled
/// for later is standing from the moment it is received, and the client checks the window.
/// </para>
/// <para>
/// Receiving never lets a fact about a retracted announcement reach the stream (see <see cref="MarqueeInbox"/>), so the
/// reducer needs no memory of what it removed.
/// </para>
/// </remarks>
/// <param name="Id">The announcement.</param>
/// <param name="Audience">Who it is for.</param>
/// <param name="Title">Its title.</param>
/// <param name="Body">What it says.</param>
/// <param name="Display">How it is shown.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
public record StandingAnnouncement(
    AnnouncementId Id,
    Audience Audience,
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Display Display,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil)
{
    /// <summary>
    /// Holds the announcement, unless it is already held.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static StandingAnnouncement Reduce(AnnouncementReceived @event, StandingAnnouncement? current, EventContext context) =>
        current ?? new(
            new AnnouncementId(Guid.Parse(context.EventSourceId.Value)),
            @event.Audience,
            @event.Title,
            @event.Body,
            @event.Display,
            @event.Importance,
            @event.ActiveFrom,
            @event.ActiveUntil);

    /// <summary>
    /// Revises what the announcement says and when it is shown.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static StandingAnnouncement? Reduce(AnnouncementRevisionReceived @event, StandingAnnouncement? current) =>
        current is null ? null : current with { Title = @event.Title, Body = @event.Body, Importance = @event.Importance, ActiveFrom = @event.ActiveFrom, ActiveUntil = @event.ActiveUntil };

    /// <summary>
    /// Gives the announcement another audience.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static StandingAnnouncement? Reduce(AnnouncementAudienceChangeReceived @event, StandingAnnouncement? current) =>
        current is null ? null : current with { Audience = @event.Audience };

    /// <summary>
    /// Removes the announcement: it was retracted.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>Nothing: the announcement is gone.</returns>
    public static StandingAnnouncement? Reduce(AnnouncementRetractionReceived @event, StandingAnnouncement? current) => null;
}
