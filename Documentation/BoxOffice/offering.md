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
    .Seats(SeatLicensing.PerUser));
```

## What a declaration holds

| Part | What it says |
| --- | --- |
| Product | The identity the operator knows the product by. The declaration is appended under it. |
| Module | A key, a name, the modules it `Requires`, the code namespaces it `Owns`, its default `TrialDays` and its availability: `Included`, `Paid`, `Preview` or `Coming`. |
| Meter | A key, a name, the unit it counts in and whether the product also limits how much a tenant uses at once. |
| Seats | Whether people need a seat (`NotLicensed` or `PerUser`), and for which tenant roles. No roles means every role. |

`ProductOffering.ModuleOwning` finds the module that owns a namespace. The module that owns the longest matching namespace wins, and `Acme.Analytics` does not own `Acme.AnalyticsExtra`.

## A declaration that cannot be sold stops the product

`AddBoxOfficeOffering` checks the declaration when it is built. A module that requires one the product does not declare, a module that requires itself, modules that require each other, a namespace owned by two modules, a negative trial, a duplicate key and roles that need a seat where seats are not licensed throw `InvalidOffering` with one sentence per problem, so the product does not start.

## Reported once per change

The declaration goes to the outbox of the product's default namespace when the product starts, and again only when it differs from the last one reported:

- Two starts with the same declaration report once, also when several instances start together.
- The order modules and meters are declared in does not matter.
- Declaring A, then B, then A again is three reports, because each replaces a different last declaration.

What was last reported is read from the outbox itself, so the answer survives restarts. If Chronicle is not up yet, the product keeps trying every `OfferingOptions.RetryInterval` (30 seconds by default) without holding up its start.
