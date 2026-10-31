---
title: Metering compute
description: Measure a product's compute per tenant, meter and day from the product's own events, and report each day once it is closed, with a correction when usage arrives late.
---

`Cratis.Showrunner.BoxOffice.Offering` also measures what a tenant used. A meter reads the events the product already records, such as the end of a session. Box Office adds no second "session started" event.

## Declare the meter, then measure it

A meter is declared in the [offering](./offering.md) so it can be sold, and measured by an `IMeter` that turns one of the product's events into a quantity.

```csharp
builder.Services.AddBoxOfficeOffering(offering => offering
    .Product(productId)
    .Meter("compute", "Compute", "minutes"));

builder.Services.AddBoxOfficeMetering();
builder.Services.AddMeter<SessionEnded>("compute", (session, context) =>
    new MeterReading(session.Organization, DateOnly.FromDateTime(session.Started.UtcDateTime), session.Minutes));
```

A reading says which organization the usage is reported under, which UTC day it counts for and how much of the meter was used, in the meter's unit. Return `null` for an event that used none of it. The meter decides the day, so a session counts for the day it started even when it ends the next day. A longer meter is a class that implements `IMeter` and is registered with `AddMeter<TMeter>()`.

A meter that is not declared, a meter registered twice, a meter that reads no events, and a declared key that contains `.` or starts with `$` throw `InvalidMetering` when the product starts.

## Where the totals live

Running totals are metering buckets beside the event log, not events. `AddBoxOfficeMetering` keeps them in one collection of the product's MongoDB database, which must be registered as an `IMongoDatabase`; `MeteringOptions.Collection` names it. A tenant is a namespace of the product's event store, so Box Office measures every namespace except the default one, and the tenant is the name of the namespace.

Each tenant's day is one document. An event is added to a bucket once, so reading the same events again after a restart counts nothing twice. Only one instance of the product measures at a time, the one holding a lease in the same collection. The others take over when its lease (`MeteringOptions.Lease`, 2 minutes) runs out.

## A day closes once

A UTC day closes 6 hours after midnight (`MeteringOptions.CloseDelay`). The first measurement after that appends `ComputeDayClosed(TenantId, Day, Usage)` to the outbox of the tenant's namespace, under the organization, with the total of each meter that has a bucket. The event carries an idempotency key made from the tenant and the day, and the store marks the day reported only after the event store has it. A product that stops between the two reports nothing twice.

## A session that ends after its day closed is a correction

A session that ends after its day closed still counts for the day it started. Box Office adds it to the bucket and appends `ComputeDayCorrected` with the corrected totals of the day, not the difference, as [the contracts](./contracts.md) say. Each late session produces one correction, and a late session that is read twice produces it once.

## Admission reads the buckets

The buckets are what [compute admission](./compute-admission.md) compares with a tenant's allowance before it lets work start.

## Metrics

Metering exposes counters under the meter named after `Cratis.Showrunner.BoxOffice.Offering.Metering.TenantMetering`. Subscribe to it with `AddMeter` in the OpenTelemetry setup.

| Counter | Counts |
| --- | --- |
| `boxoffice.metering.buckets` | Buckets updated with a quantity, tagged with `boxoffice.meter` |
| `boxoffice.metering.days.closed` | Days reported as closed |
| `boxoffice.metering.corrections` | Corrections reported for a day that was already closed |
