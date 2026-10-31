// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.given;

/// <summary>
/// Events the specifications seed an offer with.
/// </summary>
public static class OfferEvents
{
    /// <summary>
    /// A seat plan of ten seats, defined.
    /// </summary>
    public static readonly object[] Team =
    [
        new OfferDefined(OfferKind.SeatPlan, "Team", "For small teams."),
        new SeatPlanSpecified(10, false, false)
    ];

    /// <summary>
    /// A seat plan with unlimited seats, defined.
    /// </summary>
    public static readonly object[] Enterprise =
    [
        new OfferDefined(OfferKind.SeatPlan, "Enterprise", "For everyone."),
        new SeatPlanSpecified(0, true, true)
    ];

    /// <summary>
    /// A module offer, defined.
    /// </summary>
    public static readonly object[] Reporting =
    [
        new OfferDefined(OfferKind.Module, "Reporting", "Reports on everything."),
        new ModuleOfferSpecified("reporting", ModuleCharging.PerSeat, 14, ModuleAvailability.Paid)
    ];

    /// <summary>
    /// A compute pack, defined.
    /// </summary>
    public static readonly object[] ExtraBuilds =
    [
        new OfferDefined(OfferKind.ComputePack, "Extra builds", "More build minutes."),
        new ComputePackSpecified("build-minutes", 1000, true)
    ];

    /// <summary>
    /// A monthly price of 100.00 USD with a 20 percent yearly discount.
    /// </summary>
    public static readonly OfferPriced Priced = new("USD", 10000, 20m);

    /// <summary>
    /// Joins sets of events and single events into one list, in order.
    /// </summary>
    /// <param name="events">Events, or arrays of them.</param>
    /// <returns>The events.</returns>
    public static object[] Then(params object[] events) =>
        [.. events.SelectMany(@event => @event is object[] many ? many : [@event])];
}
#endif
