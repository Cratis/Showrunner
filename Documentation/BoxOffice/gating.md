---
title: Module gating
description: How a product refuses the commands, queries and reactors of a module the tenant does not have, and when a trial or grant stops admitting.
---

A product sells its modules and the operator decides which a tenant has. Module gating makes that decision hold: a command, a query or a reactor that belongs to a module is refused for a tenant that does not have the module.

## A module owns namespaces

The product declares which code namespaces each module owns, in its [offering](./offering.md):

```csharp
builder.Services.AddBoxOfficeOffering(offering => offering
    .Product(productId)
    .Module("core", "Core", module => module.Availability(ModuleAvailability.Included).Owns("Acme.Core"))
    .Module("analytics", "Analytics", module => module.Owns("Acme.Analytics").TrialDays(14)));
```

Everything inside an owned namespace belongs to the module, however deep: `Acme.Analytics.Reports.Export` is analytics, and `Acme.AnalyticsExtra` is not. A namespace nobody owns belongs to no module and is never gated. A module declared `Included` is part of the product and is always admitted.

Gating needs the offering. A product that calls `AddBoxOffice` but declares none has no module that owns a namespace, so it admits everything.

## What is refused

| What | How it is refused |
| --- | --- |
| A command in the module's namespace | Not authorized, with the reason `ModuleNotEntitled`. |
| A query whose name is in the module's namespace | Unauthorized, and the client is told no more than that. |
| A reactor that asks `Admits(this)` | It skips the event. |

The command and query filters run before the command or query does anything. They refuse the product's own background flows as well, because a module the tenant does not have acts for nobody. The reason is the constant `GatingReasons.ModuleNotEntitled`, so a client and a specification match the reason rather than presentation text. The refusal is logged with the module that was missing.

A gated reactor asks the gate before it acts:

```csharp
public class ReportMailer(IModuleGate gate) : IReactor
{
    public async Task Sent(ReportScheduled @event, EventContext context)
    {
        if (!await gate.Admits(this))
        {
            return;
        }

        // ...
    }
}
```

`Admits` accepts the reactor or its `Type` and answers for the tenant of the current dependency-injection scope, the same scope the reactor's other tenant-scoped services such as `IEntitlements` resolve in.

## What counts as having a module

The gate asks [`IEntitlements`](./entitlements.md), so a module counts when the tenant has it in full or on trial. A preview does not count: it only lets the tenant look at a module it does not have.

Nothing that ends needs an event to end it. A module trial or a grant that was given until a day admits through that day, and the next day it is refused, computed from the clock when the command or query arrives. A revoked module is refused at once. A module that `Requires` another is admitted on its own entitlement: the prerequisites are checked when the offering is declared, and the operator decides what a tenant gets.

## Turning enforcement off

Enforcement is on by default. Set `EnforceModules` to `false` to admit everything, for a deployment whose tenants have not been given their entitlements yet. It is an operational escape hatch and nothing else:

```csharp
builder.AddBoxOffice(boxOffice => boxOffice.EnforceModules = false);
```

## Specifying gated code

Specify a gated command or query against the filter's gate, with the entitlements of the tenant substituted, and specify the gate itself with the tenant's decisions given as the operator's facts. Box Office's own specifications do both, including the expired trial that is refused without any event.
