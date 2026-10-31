---
title: Catalog
description: The Box Office catalog - the offers an operator such as Direct sells, how they are priced, the pricing arithmetic, and how the catalog is published to products.
---

`Cratis.Showrunner.BoxOffice.Catalog` holds what the operator of a managed product sells and what it costs. It is a library the operator hosts in its Arc application and Chronicle event store, next to [the contracts](./contracts.md) it publishes through. A product never references it: a product reads the `OfferPublished` and `OfferWithdrawn` events the catalog publishes.

Every command and query in the catalog is `[Authorize]`: it needs a signed-in caller. Restrict who may change the catalog with the operator's own authorization, for example a role policy on the host, the same way as for any other Arc command.

## Offers

An offer is an event stream under an `OfferId`, and there are three kinds.

| Kind | What it sells | Defined with |
| --- | --- | --- |
| `SeatPlan` | Seats, or unlimited seats, plus the modules and compute allowances it includes. Can be bought through sales instead of checkout. | `DefineSeatPlan` |
| `Module` | One module on its own, charged flat per organization or per seat, with a trial length in days, as paid or as a preview. | `DefineModuleOffer` |
| `ComputePack` | An extra quantity of a meter per period, optionally stackable. | `DefineComputePack` |

An offer is defined once. After that these commands change it, and each refuses an offer that is not defined or is retired:

| Command | Effect |
| --- | --- |
| `PriceOffer` | Sets the currency (a three-letter code), the monthly price in minor units and the yearly discount (0 to 100 percent). Setting the price an offer already has records nothing. |
| `IncludeModuleInOffer` | A seat plan includes a module. Including a module it already includes records nothing. |
| `IncludeAllowanceInOffer` | A seat plan includes a compute allowance. One allowance holds per meter and period; including another replaces it, and including the same one records nothing. |
| `RetireOffer` | The offer is no longer sold. It stays in the catalog, retired, because tenants that bought it keep it. |

The catalog's own events are private to the operator: `OfferDefined`, `SeatPlanSpecified`, `ModuleOfferSpecified`, `ComputePackSpecified`, `OfferPriced`, `OfferIncludedModule`, `OfferIncludedAllowance` and `OfferRetired`. The `Offer` read model folds them, and its queries `AllOffers` (retired offers included) and `OffersOnSale` (priced and not retired) observe the catalog by name.

## Pricing

`Pricing` holds the arithmetic Direct's Admin catalog used, moved here and expressed in minor units (`PriceInMinorUnits`, cents rather than a decimal). Every result is a whole number of minor units, rounded away from zero at the midpoint.

| Method | Result |
| --- | --- |
| `TotalFor(monthlyPrice, yearlyDiscount, interval)` | A monthly period costs the monthly price. A yearly period costs twelve months less the yearly discount. |
| `PeriodEnd(start, interval)` | The day the next period starts: the start one month or one year later, clamped to the last day of a shorter month. |
| `Prorate(recurringAmount, currentQuantity, addedQuantity, periodEnd, interval, today)` | What adding a quantity, such as seats, costs for the days left of the current period. Nothing when no quantity is paid for yet or the period is over; the whole price when the period has not started. |

## Moving an existing seat SKU

An existing seat SKU keeps its meaning as a seat plan offer. `SeatPlanOffers.From(sku, pricing)` returns the events that define a `SeatSku` (name, description, seats, unlimited and whether it is bought through sales) and its `SeatSkuPricing` (currency, monthly price, yearly discount) as a seat plan offer. Append them under the offer's id. The offer carries the same seats, sales route and price, and costs the same per month and per year. `SeatPlanOffers.From(sku)` defines a SKU that has no price yet.

## Publishing the catalog

`OfferPublisher` is a Chronicle reactor. When what an offer says to a product changes, it appends `OfferPublished` to the outbox of the namespace the offer was recorded in, under the same `OfferId`. When a published offer is retired, it appends `OfferWithdrawn`.

- **An offer has a public face only while it is priced and not retired.** Defining an offer, or giving it seats, publishes nothing until it is priced.
- **A monthly and a yearly price are published**, the yearly one from `Pricing.TotalFor`, in the offer's currency.
- **A module offer publishes its module** in `Modules`. A seat plan publishes the modules it includes and its seats. A compute pack publishes its name and prices, with no seats and no modules.
- **An unlimited seat plan publishes `int.MaxValue` seats**, because the contract has no separate flag. `OfferPublications.UnlimitedSeats` names the value.
- **Allowances are not published.** `OfferPublished` has no field for them, so including an allowance publishes nothing.
- **Only a change is published.** The publisher folds the offer's own events from the event log, up to and including the delivered event and up to but excluding it, and publishes when the two public faces differ. Setting the price an offer already has publishes nothing.
- **A delivery is published once.** Each publication carries an idempotency key derived from the delivery, and the reactor is `[OnceOnly]`, so a redelivery or a replay does not publish it again.
