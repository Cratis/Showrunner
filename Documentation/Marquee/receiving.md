---
title: Receiving
description: How a product receives announcements from the authoring store once, records them product-wide, and decides per tenant which ones its audience matches.
---

`Cratis.Showrunner.Marquee` is the package a product references to show announcements. It also runs in the product's own Arc and Chronicle process. It receives the [contract events](./contracts.md) from the authoring store, records them once for the whole product, and answers, per tenant and per person, which announcements to show.

## Add Marquee to a product

Reference the `Cratis.Showrunner.Marquee` package and call `AddMarquee` next to `AddCratis`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddMarquee(marquee =>
{
    marquee.Product = "ledger";
    marquee.AuthoringStores = ["Direct"];
});
```

| Option | Default | Meaning |
| --- | --- | --- |
| `Product` | none, required | The product this application is, as announcements name it. An authoring store serves several products; the product records only its own. |
| `AuthoringStores` | `["Direct"]` | The names of the event stores whose outbox the announcements come from, which is where [authoring](./authoring.md) runs. |
| `PollingInterval` | one minute | How often Marquee makes sure the subscriptions exist and the inbox is observed. |

The host fails to start when `Product` is not set. A module or plan audience is answered by the `IAudienceEntitlements` in the service collection; add Box Office's before or after `AddMarquee` and it is used. Without one, only `Everyone` and `Tenants` audiences reach anybody; see [Plugging in who holds a module](./contracts.md#plugging-in-who-holds-a-module-or-is-on-a-plan).

## The subscription

The contracts name no store, so nothing routes them on its own. For each authoring store, `AddMarquee` adds a hosted service that

- subscribes the product's event store to the authoring store's outbox, for the four announcement events, under the subscription id `marquee-<store>` (so it does not collide with a subscription another part such as Usher makes to the same store), and
- observes the `inbox-<store>` event sequence those events arrive in.

Both steps are idempotent and repeat every `PollingInterval`, so a restart or an authoring store that was not ready yet is caught up by the next round. The inbox reactor does not replay: replaying it would only receive again what is already recorded.

## Recorded once, product-wide

An announcement is the same for every tenant, so it is recorded **once**, in the product's own (default) namespace, and not copied to each tenant's namespace. A tenant provisioned after an announcement was published still sees it while it is active.

`MarqueeInbox` turns each public event into a private fact and appends it under the announcement's id, with event source type `marquee`:

| Public event | Recorded as |
| --- | --- |
| `AnnouncementPublished` | `AnnouncementReceived` |
| `AnnouncementRevised` | `AnnouncementRevisionReceived` |
| `AnnouncementAudienceChanged` | `AnnouncementAudienceChangeReceived` |
| `AnnouncementRetracted` | `AnnouncementRetractionReceived` |

A reducer folds them into `StandingAnnouncement`, one per announcement the product holds. A retraction removes it.

- **Delivered at least once, recorded once.** The fact is appended with an idempotency key derived from the event: the authoring store, the announcement, the event type, when it occurred, its correlation id and its content. Every copy of the same event yields one fact.
- **Another product's announcements are ignored.** An event naming a product other than `Product` records nothing.

### A retraction always wins

An announcement that was retracted is never brought back, whatever order its events arrive in. The inbox records a retraction even for an announcement it never received, and drops what arrives about the announcement after it: a publication, a revision or a new audience. It also drops a revision or a new audience for an announcement it never received, and a second publication of one it holds. All announcements of a product arrive under one event source, so the inbox handles them one at a time in the order they were published.

## What a tenant sees

`AnnouncementForMe.AnnouncementsForMe` is an observable query that returns the announcements the signed-in person is to see in the current tenant. It resolves the tenant from Arc's tenancy, keeps the standing announcements whose audience includes it, and leaves out a `DialogOnce` announcement the person already [acknowledged](./acknowledging.md). Nobody who is not signed in is authorized.

The query does not filter by date. An announcement scheduled for later is returned, with `ActiveFrom` and `ActiveUntil`, and the client compares the window with the clock every 60 seconds, so it appears on time without a round trip.
