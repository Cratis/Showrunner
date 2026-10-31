// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.given;

/// <summary>
/// The announcements and facts the specifications seed the product with.
/// </summary>
public static class ProductAnnouncements
{
    /// <summary>
    /// The product the specifications run as.
    /// </summary>
    public static readonly Product Product = new("ledger");

    /// <summary>
    /// The tenant the specifications run as.
    /// </summary>
    public static readonly TenantId Tenant = new("fjord");

    /// <summary>
    /// Another tenant of the product.
    /// </summary>
    public static readonly TenantId AnotherTenant = new("harbor");

    /// <summary>
    /// A maintenance window, on 20 November 2026 between 02:00 and 04:00 UTC.
    /// </summary>
    public static readonly DateTimeOffset MaintenanceStarts = new(2026, 11, 20, 2, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// The end of the maintenance window.
    /// </summary>
    public static readonly DateTimeOffset MaintenanceEnds = new(2026, 11, 20, 4, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Gets a banner about the maintenance window.
    /// </summary>
    /// <param name="id">The id of the banner.</param>
    /// <param name="audience">Who the banner is for; everyone by default.</param>
    /// <returns>The banner.</returns>
    public static StandingAnnouncement Banner(AnnouncementId id, Audience? audience = null) => new(
        id,
        audience ?? Audience.Everyone(),
        "Scheduled maintenance",
        "Ledger is read-only for two hours.",
        Display.Banner,
        Importance.Warning,
        MaintenanceStarts,
        MaintenanceEnds);

    /// <summary>
    /// Gets a dialog shown once, without a window.
    /// </summary>
    /// <param name="id">The id of the dialog.</param>
    /// <param name="audience">Who the dialog is for; everyone by default.</param>
    /// <returns>The dialog.</returns>
    public static StandingAnnouncement DialogOnce(AnnouncementId id, Audience? audience = null) => new(
        id,
        audience ?? Audience.Everyone(),
        "Meet Reporting",
        "Reports on everything.",
        Display.DialogOnce,
        Importance.Information,
        ActiveFrom.Immediately,
        ActiveUntil.Indefinitely);

    /// <summary>
    /// Gets a dialog shown at the start of every session.
    /// </summary>
    /// <param name="id">The id of the dialog.</param>
    /// <returns>The dialog.</returns>
    public static StandingAnnouncement DialogEverySession(AnnouncementId id) => DialogOnce(id) with { Display = Display.DialogEverySession, Title = "Release notes" };

    /// <summary>
    /// Gets the public event the authoring store sends for an announcement being published.
    /// </summary>
    /// <param name="announcement">The announcement.</param>
    /// <param name="product">The product it is for; the product of the specifications by default.</param>
    /// <returns>The public event.</returns>
    public static AnnouncementPublished Published(StandingAnnouncement announcement, Product? product = null) => new(
        announcement.Id,
        product ?? Product,
        announcement.Audience,
        announcement.Title,
        announcement.Body,
        announcement.Display,
        announcement.Importance,
        announcement.ActiveFrom,
        announcement.ActiveUntil);

    /// <summary>
    /// Gets the fact the product records when it receives an announcement.
    /// </summary>
    /// <param name="announcement">The announcement.</param>
    /// <returns>The fact.</returns>
    public static AnnouncementReceived Received(StandingAnnouncement announcement) => new(
        announcement.Audience,
        announcement.Title,
        announcement.Body,
        announcement.Display,
        announcement.Importance,
        announcement.ActiveFrom,
        announcement.ActiveUntil);

    /// <summary>
    /// Joins sets of events and single events into one list, in order.
    /// </summary>
    /// <param name="events">Events, or arrays of them.</param>
    /// <returns>The events.</returns>
    public static object[] Then(params object[] events) =>
        [.. events.SelectMany(@event => @event is object[] many ? many : [@event])];
}
#endif
