---
title: Offering
description: Declare in code what a product sells - modules, meters and seat licensing - and have Box Office report it to the operator once per change.
---

`Cratis.Showrunner.BoxOffice.Offering` lets a managed product say what it sells. The product declares its modules, its meters and how it licenses seats at startup, and Box Office reports that declaration to the operator as `OfferingDeclared`. A deploy that adds a module makes it sellable without a manual step.

```csharp
builder.Services.AddBoxOfficeOffering(offering => offering
    .Product(Guid.Parse("6f9b2d3e-4f7a-4c52-9a4f-3c3f1d2a9e10"))
    .Module("core", "Core", module => module.Availability(ModuleAvailability.Included).Owns("Acme.Core"))
    .Module("analytics", "Analytics", module => module.Requires("core").Owns("Acme.Analytics").TrialDays(14))
    .Meter("compute", "Compute", "minutes")
    .Seats(SeatLicensing.PerUser)
    .DefaultOffering("Starter"));
```

## What a declaration holds

| Part | What it says |
| --- | --- |
| Product | The identity the operator knows the product by. The declaration is appended under it. |
| Module | A key, a name, the modules it `Requires`, the code namespaces it `Owns`, its default `TrialDays` and its availability: `Included`, `Paid`, `Preview` or `Coming`. |
| Meter | A key, a name, the unit it counts in and whether the product also limits how much a tenant uses at once. |
| Seats | Whether people need a seat (`NotLicensed` or `PerUser`), and for which tenant roles. No roles means every role. |
| Default offering | Optional. The name of the [offer](./catalog.md) in the operator's catalog a new tenant starts on. See [Starting a tenant on the default offering](#starting-a-tenant-on-the-default-offering). |

`ProductOffering.ModuleOwning` finds the module that owns a namespace. The module that owns the longest matching namespace wins, and `Acme.Analytics` does not own `Acme.AnalyticsExtra`.

## A declaration that cannot be sold stops the product

`AddBoxOfficeOffering` checks the declaration when it is built. A module that requires one the product does not declare, a module that requires itself, modules that require each other, a namespace owned by two modules, a negative trial, a duplicate key, roles that need a seat where seats are not licensed and a default offering without a name throw `InvalidOffering` with one sentence per problem, so the product does not start.

## Reported once per change

The declaration goes to the outbox of the product's default namespace when the product starts, and again only when it differs from the last one reported:

- Two starts with the same declaration report once, also when several instances start together.
- The order modules and meters are declared in does not matter.
- Declaring A, then B, then A again is three reports, because each replaces a different last declaration.

What was last reported is read from the outbox itself, so the answer survives restarts. If Chronicle is not up yet, the product keeps trying every `OfferingOptions.RetryInterval` (30 seconds by default) without holding up its start.

When the product declares a default offering, it is reported the same way, as `DefaultOfferingDeclared`, once per change, so the operator knows which offer new tenants start on.

## Starting a tenant on the default offering

A product that sells itself starts a new tenant on its default offering without waiting for the operator. The product decides, the operator is told, and the operator can change what the tenant has afterwards. There is no confirmation step.

Call `IDefaultOffering.StartTenant` when the product has created the tenant:

```csharp
public class Signups(IDefaultOffering defaultOffering)
{
    public Task StartOnTheDefaultOffering(TenantId tenant, OrganizationId organization) =>
        defaultOffering.StartTenant(tenant, organization);
}
```

`AddBoxOfficeOffering` registers `IDefaultOffering`. It finds the offer named in `DefaultOffering(...)` among the offers the operator published (`AddBoxOffice` supplies them) and does two things:

- It records `TenantStartedOnDefaultOffering` in the tenant's own stream. The tenant has the offer's included seats and modules at once, so [gating](./gating.md) admits the modules and [seats](./seats.md) can be assigned without waiting for the operator. See [Entitlements](./entitlements.md#how-a-decision-reaches-the-tenant). The catalog publishes no trial length, so the tenant starts without a trial.
- It tells the operator `TenantStartedOnOffer`, with the offer's name, its included seats and its modules. The fact goes to the outbox of the tenant's namespace, under the organization, like every other [tenant fact the product publishes](./contracts.md#product-to-operator).

A later decision of the operator, such as `SeatCapacitySet` or `ModuleRevoked`, replaces what the tenant started with, because the newest decision wins.

- **An unknown name fails where it is used.** The catalog arrives from the operator after the product starts, so a name is checked when a tenant starts, not at startup. `StartTenant` throws `DefaultOfferingNotPublished` when the operator has not published an offer with that name or has withdrawn it, and `DefaultOfferingNotDeclared` when the product declared no default offering. Nothing is published then. The name is compared without regard to case.
- **Starting a tenant twice tells the operator once.** Delivery is at least once, so calling `StartTenant` again for the same tenant, also from another instance, returns `TenantStart.AlreadyStarted` and publishes nothing.
- **Both facts are recorded once.** If the product stops between the two, starting the tenant again completes what is missing.
- **A different default later does not restart a tenant.** A tenant that started on one offer stays started; the operator changes what it has with its own decisions.

## The product's event store

Box Office records in the product's own event store. In a host set up with `AddCratis` or `AddCratisChronicle`, that is the application's event store, and nothing needs naming. Set `OfferingOptions.EventStore`, or `BoxOfficeOptions.EventStore` when Box Office is added, only to use another store. The same store is used for reporting the offering, [metering](./metering.md), starting tenants on the default offering and for everything `AddBoxOffice` reads and records. A host that registers only the Chronicle client, without an application event store, uses the name in `ChronicleClientOptions.EventStore`, as before.
