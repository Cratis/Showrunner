---
title: Audience routing
description: Address what products and operators publish to each other, so several products on one operator store each receive only what is meant for them
---

A Chronicle event store's outbox is a broadcast: every store that subscribes to it receives every event of the types it asks for. When one operator runs several products on one store, each product would otherwise receive the decisions meant for the others. An **audience** is a tag on an outbox event, named `audience`, that a subscription can filter on. The mechanism is Chronicle's: `EventTags.Audience(...)` addresses an append, and `ForAudience(...)` on a subscription filters on it.

Audience routing is opt-in everywhere. Every option below defaults to unset, which means nothing is addressed and every subscription is unfiltered: a deployment that does not set them behaves exactly as before. Both sides have to agree: a subscription that filters on an audience receives nothing that was published without one, so set the publisher's option before, or together with, the subscriber's.

## Which channels are addressed

| Channel | Direction | Addressing |
| --- | --- | --- |
| Operator decisions (`TrialOpened`, `SeatCapacitySet`, `ModuleGranted`, `TenantSuspended`, `OfferPublished`, ...) | operator to every product | Product subscribes for its own audience: `BoxOfficeOptions.ProductAudience`. The operator addresses what it publishes. |
| Offering, default offering, started tenants (`OfferingDeclared`, `DefaultOfferingDeclared`, `TenantStartedOnOffer`) | product to operator | Addressed to the operator: `OperatorAudience`. |
| Seat reports (`SeatTaken`, `SeatReleased`) and compute reports (`AllowanceExhausted`, `ComputeDayClosed`, `ComputeDayCorrected`) | product to operator | Addressed to the operator: `OperatorAudience`. |
| Support desk traffic (Usher) | both | `UsherOptions.Audience` filters; `UsherOptions.DeskAudience` addresses. |
| Notes published to a collector | application to collector | `NotesOptions.CollectorAudience` addresses; `NotesOptions.Audience` filters what comes back and names the sender. |
| Collector replies to a note | collector to application | Addressed automatically to the application that named itself with `NotesOptions.Audience`. |
| Marquee announcements | authoring to a product | `MarqueeAuthoringOptions.AddressToProducts` addresses by product; `MarqueeOptions.SubscribeForProduct` filters. |
| GuestList audience grants | operator to a product | `GuestListAuthoringOptions.AddressToProducts` addresses by product; `GuestListOptions.SubscribeForProduct` filters. The routing audience is separate from the `Audience` field of the events. |

Two kinds of traffic are left alone on purpose. `TenantRegistered` and `TenantProvisioned` are one-to-one between an operator and the product that owns the tenant, and the event source (the organization) already identifies the pair. The Notes HTTP relay moves one application's outbox to one collector and has no other recipient to tell apart. Note replies from the collector to an application are not addressed yet; see the limits below.

## Box Office

```csharp
builder.AddBoxOffice(boxOffice =>
{
    boxOffice.OperatorStores = ["Direct"];
    boxOffice.ProductAudience = "studio";   // receive only what the operator addresses to "studio"
    boxOffice.OperatorAudience = "direct";  // address every report to the operator "direct"
});
```

| Option | Default | Meaning |
| --- | --- | --- |
| `ProductAudience` | unset | The audience that identifies this product. When set, the subscription to each operator store carries `audience=<value>`, so the operator's events for other products never reach this product's inbox. |
| `OperatorAudience` | unset | The audience of the operator the product reports to. When set, everything the product appends to its outbox for the operator is tagged with it. It also sets `OfferingOptions.OperatorAudience`, which is what the offering declaration, the default offering, the started tenants and the metering use. |

A blank value is refused when the host starts. With several `OperatorStores`, one `OperatorAudience` addresses everything the product reports, whichever operator reads it.

A product that uses only `AddBoxOfficeOffering` or `AddBoxOfficeMetering`, without `AddBoxOffice`, sets the operator audience on `OfferingOptions`:

```csharp
builder.Services.Configure<OfferingOptions>(options => options.OperatorAudience = "direct");
```

The operator subscribes to the product's outbox for itself, with the Chronicle subscription builder:

```csharp
await eventStore.Subscriptions.Subscribe(
    new EventStoreSubscriptionId("studio"),
    "Studio",
    subscription => subscription.ForAudience("direct"));
```

## Usher, Notes and Marquee

The same two ideas apply: the receiving side filters with an option, the publishing side addresses with another.

```csharp
builder.Services.AddUsher(usher =>
{
    usher.Audience = "studio";      // receive what the desk addresses to this application
    usher.DeskAudience = "direct";  // address what Usher publishes to the desk
});

builder.Services.AddNotes(notes =>
{
    notes.Audience = "studio";
    notes.CollectorAudience = "direct";
});

builder.Services.AddMarquee(marquee => marquee.SubscribeForProduct = true);   // filters on the value of Product
services.Configure<MarqueeAuthoringOptions>(options => options.AddressToProducts = true);
```

Marquee's own `Audiences` (a module or plan audience that decides who sees an announcement) are a different thing: they are a property of the announcement, not a tag on the outbox event.

## What is not marked `[Audience]`

Chronicle's `[Audience]` on an event type makes an audience mandatory: appending the event without one fails. No contract event is marked, because every option above is opt-in and a product that has not set one would start failing to publish. A host that wants the guarantee marks its own events, or adds its own `IEventTagEnricher`.

## Limits

- A subscription that changes its filter applies the new filter to events from then on; events already forwarded stay in the inbox.
- Chronicle filters in the kernel, so the kernel has to be a version that supports filtered subscriptions.
- A note carries the sending application's `NotesOptions.Audience` as a `from` tag. The collector keeps it on the private fact it records and addresses `OperatorAcknowledgedNote`, `OperatorAddressedNote` and `OperatorDeclinedNote` to it, so an application that sets `NotesOptions.Audience` receives the answers to its own notes. A note from an application that set no `Audience` is answered unaddressed, so an application that wants its subscription filtered must set `Audience`; a collector that serves several applications needs every one of them to set it.
- Notes over the HTTP transport relays event content, not tags, so `from` does not reach the collector and its answers come back unaddressed. Audience routing for notes applies to the Chronicle transport.
