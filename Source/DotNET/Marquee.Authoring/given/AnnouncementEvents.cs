// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Marquee.Authoring.given;

/// <summary>
/// Events and values the specifications seed an announcement with.
/// </summary>
public static class AnnouncementEvents
{
    /// <summary>
    /// The product the specifications author for.
    /// </summary>
    public static readonly Product Product = new("ledger");

    /// <summary>
    /// A maintenance window, on 20 November 2026 between 02:00 and 04:00 UTC.
    /// </summary>
    public static readonly DateTimeOffset MaintenanceStarts = new(2026, 11, 20, 2, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// The end of the maintenance window.
    /// </summary>
    public static readonly DateTimeOffset MaintenanceEnds = new(2026, 11, 20, 4, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// A warning banner about the maintenance window, for everyone.
    /// </summary>
    public static readonly AnnouncementAuthored Maintenance = new(
        Product,
        Audience.Everyone(),
        "Scheduled maintenance",
        "Ledger is read-only for two hours.",
        Display.Banner,
        Importance.Warning,
        MaintenanceStarts,
        MaintenanceEnds);

    /// <summary>
    /// An announcement of a new module, shown once in a dialog, without a window.
    /// </summary>
    public static readonly AnnouncementAuthored NewModule = new(
        Product,
        Audience.Everyone(),
        "Meet Reporting",
        "Reports on everything.",
        Display.DialogOnce,
        Importance.Information,
        ActiveFrom.Immediately,
        ActiveUntil.Indefinitely);

    /// <summary>
    /// Joins sets of events and single events into one list, in order.
    /// </summary>
    /// <param name="events">Events, or arrays of them.</param>
    /// <returns>The events.</returns>
    public static object[] Then(params object[] events) =>
        [.. events.SelectMany(@event => @event is object[] many ? many : [@event])];
}
#endif
