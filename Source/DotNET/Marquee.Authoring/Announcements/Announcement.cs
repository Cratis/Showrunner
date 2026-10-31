// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements;

/// <summary>
/// An announcement as the authoring screen shows it and as the commands that change it read it.
/// </summary>
/// <remarks>
/// It is a self-reducing read model. A withdrawn announcement stays, withdrawn, so the screen can show what was
/// withdrawn; nothing can be done to it.
/// </remarks>
/// <param name="Id">The announcement.</param>
/// <param name="Product">The product it is for.</param>
/// <param name="Audience">Who it is for.</param>
/// <param name="Title">Its title.</param>
/// <param name="Body">What it says.</param>
/// <param name="Display">How it is shown.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
/// <param name="Withdrawn">Whether it was withdrawn.</param>
[ReadModel]
public record Announcement(
    AnnouncementId Id,
    Product Product,
    Audience Audience,
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Display Display,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil,
    bool Withdrawn)
{
    /// <summary>
    /// Observes the announcements of a product, withdrawn ones included, the newest window first.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <returns>The announcements of the product, again every time one changes.</returns>
    [Authorize(Policy = MarqueeAuthoring.Policy)]
    public static ISubject<IEnumerable<Announcement>> ForProduct(Product product, IReadModels readModels)
    {
        var observed = readModels.Observe<Announcement>(announcement => announcement.Product == product);
        return Subject.Create<IEnumerable<Announcement>>(
            Observer.Create<IEnumerable<Announcement>>(_ => { }, observed.OnError, observed.OnCompleted),
            observed.Select(list => (IEnumerable<Announcement>)[.. list.OrderByDescending(announcement => announcement.ActiveFrom.Value).ThenBy(announcement => announcement.Id.Value)]));
    }

    /// <summary>
    /// Authors the announcement, unless it is already authored.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Announcement Reduce(AnnouncementAuthored @event, Announcement? current, EventContext context) =>
        current ?? new(
            new AnnouncementId(Guid.Parse(context.EventSourceId.Value)),
            @event.Product,
            @event.Audience,
            @event.Title,
            @event.Body,
            @event.Display,
            @event.Importance,
            @event.ActiveFrom,
            @event.ActiveUntil,
            false);

    /// <summary>
    /// Amends what the announcement says and when it is shown.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Announcement? Reduce(AnnouncementAmended @event, Announcement? current) =>
        current is null ? null : current with { Title = @event.Title, Body = @event.Body, Importance = @event.Importance, ActiveFrom = @event.ActiveFrom, ActiveUntil = @event.ActiveUntil };

    /// <summary>
    /// Gives the announcement another audience.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Announcement? Reduce(AnnouncementRetargeted @event, Announcement? current) =>
        current is null ? null : current with { Audience = @event.Audience };

    /// <summary>
    /// Withdraws the announcement.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Announcement? Reduce(AnnouncementWithdrawn @event, Announcement? current) =>
        current is null ? null : current with { Withdrawn = true };
}
