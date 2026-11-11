// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// An offer the product sells, as the catalog shows it and as the commands that change it read it.
/// </summary>
/// <remarks>
/// It is a self-reducing read model, and the reducer is also how the catalog's publisher folds an offer's own events, so
/// what is published is exactly what the catalog shows. Nothing is nullable: the details of another kind of offer are
/// their <c>NotSet</c> value and an offer that is not priced has <see cref="Priced"/> false. A retired offer stays in the
/// catalog, retired, because tenants that bought it keep it.
/// </remarks>
/// <param name="Id">The offer.</param>
/// <param name="Kind">What it sells.</param>
/// <param name="Name">Its name.</param>
/// <param name="Description">What it is for.</param>
/// <param name="Seats">For a seat plan, how many seats it carries.</param>
/// <param name="UnlimitedSeats">For a seat plan, whether it carries unlimited seats.</param>
/// <param name="RequiresSalesContact">For a seat plan, whether it is bought through sales.</param>
/// <param name="Module">For a module offer, the module it sells.</param>
/// <param name="Charging">For a module offer, how it is charged.</param>
/// <param name="Trial">For a module offer, how many days it can be tried.</param>
/// <param name="Availability">For a module offer, whether it is paid for or previewed.</param>
/// <param name="Meter">For a compute pack, the meter it extends.</param>
/// <param name="PackQuantity">For a compute pack, how much it adds per period.</param>
/// <param name="Stackable">For a compute pack, whether more than one can be held.</param>
/// <param name="Priced">Whether it has a price.</param>
/// <param name="Currency">The currency of its price.</param>
/// <param name="MonthlyPrice">Its price per month.</param>
/// <param name="YearlyDiscount">The discount for paying yearly.</param>
/// <param name="IncludedModules">The modules it includes.</param>
/// <param name="IncludedAllowances">The compute allowances it includes.</param>
/// <param name="Retired">Whether it is no longer sold.</param>
[ReadModel]
public record Offer(
    OfferId Id,
    OfferKind Kind,
    OfferName Name,
    OfferDescription Description,
    SeatCount Seats,
    bool UnlimitedSeats,
    bool RequiresSalesContact,
    ModuleId Module,
    ModuleCharging Charging,
    TrialLength Trial,
    ModuleAvailability Availability,
    MeterId Meter,
    MeterQuantity PackQuantity,
    bool Stackable,
    bool Priced,
    CurrencyCode Currency,
    PriceInMinorUnits MonthlyPrice,
    YearlyDiscount YearlyDiscount,
    IEnumerable<ModuleId> IncludedModules,
    IEnumerable<OfferAllowance> IncludedAllowances,
    bool Retired)
{
    /// <summary>
    /// Observes every offer in the catalog, retired ones included, by name.
    /// </summary>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <returns>The offers, again every time one changes.</returns>
    [Authorize]
    public static ISubject<IEnumerable<Offer>> AllOffers(IReadModels readModels) =>
        Observed.OrderedByName(readModels.Observe<Offer>());

    /// <summary>
    /// Observes the offers that can be bought: priced and not retired.
    /// </summary>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <returns>The offers on sale, again every time one changes.</returns>
    [Authorize]
    public static ISubject<IEnumerable<Offer>> OffersOnSale(IReadModels readModels) =>
        Observed.OrderedByName(readModels.Observe<Offer>(offer => offer.Priced && !offer.Retired));

    /// <summary>
    /// Gets what an offer costs for a billing interval.
    /// </summary>
    /// <param name="interval">The billing interval.</param>
    /// <returns>The price of one period; nothing when the offer is not priced.</returns>
    public PriceInMinorUnits TotalFor(BillingInterval interval) => Pricing.TotalFor(MonthlyPrice, YearlyDiscount, interval);

    /// <summary>
    /// Defines the offer, unless it is already defined.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static Offer Reduce(OfferDefined @event, Offer? current, EventContext context) =>
        current ?? new(
            new OfferId(Guid.Parse(context.EventSourceId.Value)),
            @event.Kind,
            @event.Name,
            @event.Description,
            SeatCount.NotSet,
            false,
            false,
            ModuleId.NotSet,
            ModuleCharging.PerOrganization,
            TrialLength.NotSet,
            ModuleAvailability.Paid,
            MeterId.NotSet,
            MeterQuantity.NotSet,
            false,
            false,
            CurrencyCode.NotSet,
            PriceInMinorUnits.NotSet,
            YearlyDiscount.NotSet,
            [],
            [],
            false);

    /// <summary>
    /// Gives a seat plan its seats.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(SeatPlanSpecified @event, Offer? current) =>
        current is null ? null : current with { Seats = @event.Seats, UnlimitedSeats = @event.Unlimited, RequiresSalesContact = @event.RequiresSalesContact };

    /// <summary>
    /// Gives a module offer its module.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(ModuleOfferSpecified @event, Offer? current) =>
        current is null ? null : current with { Module = @event.Module, Charging = @event.Charging, Trial = @event.Trial, Availability = @event.Availability };

    /// <summary>
    /// Gives a compute pack its meter.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(ComputePackSpecified @event, Offer? current) =>
        current is null ? null : current with { Meter = @event.Meter, PackQuantity = @event.Quantity, Stackable = @event.Stackable };

    /// <summary>
    /// Prices the offer.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(OfferPriced @event, Offer? current) =>
        current is null ? null : current with { Priced = true, Currency = @event.Currency, MonthlyPrice = @event.MonthlyPrice, YearlyDiscount = @event.YearlyDiscount };

    /// <summary>
    /// Adds a module to what the offer includes.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(OfferIncludedModule @event, Offer? current) =>
        current?.IncludedModules.Contains(@event.Module) == false
            ? current with { IncludedModules = [.. current.IncludedModules, @event.Module] }
            : current;

    /// <summary>
    /// Adds an allowance to what the offer includes, replacing the one for the same meter and period.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(OfferIncludedAllowance @event, Offer? current) =>
        current is null
            ? null
            : current with
            {
                IncludedAllowances =
                [
                    .. current.IncludedAllowances.Where(allowance => allowance.Meter != @event.Meter || allowance.Period != @event.Period),
                    new OfferAllowance(@event.Meter, @event.Quantity, @event.Period)
                ]
            };

    /// <summary>
    /// Retires the offer.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(OfferRetired @event, Offer? current) =>
        current is null ? null : current with { Retired = true };
}
