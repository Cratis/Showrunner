---
title: Event reference
description: Every event Marquee records - the public contract events and their generations, and the private events behind them.
---

Marquee has two layers of events. The **contract events** are public: they travel between the authoring store and a product and are the compatibility promise. The **private events** are an implementation detail of each side, and are free to change as long as the contract holds. Payloads and shared types are in [Contracts](./contracts.md); this page lists the events and says what to do when one changes.

## Contract events

All are `[Public]`, in `Cratis.Showrunner.Marquee.Contracts.Announcements`, and none names an origin store.

| Event | Generation | Published when |
| --- | --- | --- |
| `AnnouncementPublished` | 1 | An announcement was authored |
| `AnnouncementRevised` | 1 | Its text, importance or window changed |
| `AnnouncementAudienceChanged` | 1 | Its audience changed |
| `AnnouncementRetracted` | 1 | It was withdrawn |

Every event carries `AnnouncementId` and `Product` in its payload. Nothing is nullable: a value that does not apply is empty, such as `ModuleKey.NotSet`, no tenants, or `ActiveUntil.Indefinitely`.

## Generations

A generation is a released shape of an event. Today every contract event is at generation 1, and none has been released, so none has needed a second.

`EventSchemas.snapshot.json` in `Cratis.Showrunner.Marquee.Contracts` records the schema of every event, and a specification compares the code with it, so a changed schema fails the build until the snapshot is regenerated on purpose. `ReleasedEventSchemas.json` lists the generations that have shipped; a listed generation stays in the snapshot and in code, and its schema never changes.

Once an event has been released, a change to its shape is a **new generation** with a migration that upcasts the old one, never an edit. Adding a member to an enum is the one change Chronicle accepts in place. See [Contracts](./contracts.md#released-generations) for how to regenerate the snapshot.

## Private events

| Where | Event | Recorded when |
| --- | --- | --- |
| Authoring | `AnnouncementAuthored` | `AuthorAnnouncement` succeeded |
| Authoring | `AnnouncementAmended` | `AmendAnnouncement` changed the text or window |
| Authoring | `AnnouncementRetargeted` | `RetargetAnnouncement` changed the audience |
| Authoring | `AnnouncementWithdrawn` | `WithdrawAnnouncement` succeeded |
| Product | `AnnouncementReceived` | `AnnouncementPublished` arrived |
| Product | `AnnouncementRevisionReceived` | `AnnouncementRevised` arrived |
| Product | `AnnouncementAudienceChangeReceived` | `AnnouncementAudienceChanged` arrived |
| Product | `AnnouncementRetractionReceived` | `AnnouncementRetracted` arrived |
| Product, per tenant | `AnnouncementAcknowledged` | A person acknowledged a dialog shown once |

The authoring events are mapped to the contract in [Authoring](./authoring.md#the-outbox), the product's in [Receiving](./receiving.md#recorded-once-product-wide), and `AnnouncementAcknowledged` in [Acknowledging](./acknowledging.md).

## The frontend types

The generated TypeScript types follow the C# ones. `Display` is `banner`, `dialogOnce` and `dialogEverySession`, and `Importance` is `information`, `warning` and `critical`, both exported from `@cratis/showrunner.marquee` and `@cratis/showrunner.marquee.authoring`. See [Frontend proxies](./frontend-proxies.md).
