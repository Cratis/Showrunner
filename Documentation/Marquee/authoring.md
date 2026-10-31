---
title: Authoring
description: The commands and query for writing announcements, the private events they record, how the outbox publishes them to products, and what happens when a withdrawal overtakes a publication.
---

`Cratis.Showrunner.Marquee.Authoring` is the backend for the screen where the people running a product write announcements. It runs in the authoring application's own Arc and Chronicle process, as [Usher](../Usher/backend.md) does: an Arc command records a private event, and a reactor publishes it to products as one of the [contract events](./contracts.md).

## Add authoring to an application

Reference the `Cratis.Showrunner.Marquee.Authoring` package. Arc discovers the commands and the query, and Chronicle the events, the read model and the outbox reactor, from the referenced assembly; there is nothing to register except who may author.

Every command and the query carry `[Authorize(Policy = MarqueeAuthoring.Policy)]`. Authorization is the host's decision, so the package defines the policy name and nothing else: register a policy of that name that admits your operators.

```csharp
builder.Services.AddArcAuthorizationPolicy<OperatorsOnly>(MarqueeAuthoring.Policy);
```

A host that does not register the policy can not use the authoring slices.

## Commands

Each slice is a folder with the command, its validator and its events. An announcement has its own event stream, its `AnnouncementId`, in the event store where it is authored.

| Slice | Command | What it records |
| --- | --- | --- |
| `AuthorAnnouncement` | `AuthorAnnouncement(AnnouncementId, Product, Audience, Title, Body, Display, Importance, ActiveFrom, ActiveUntil)` | `AnnouncementAuthored` |
| `AmendAnnouncement` | `AmendAnnouncement(AnnouncementId, Title, Body, Importance, ActiveFrom, ActiveUntil)` | `AnnouncementAmended` |
| `RetargetAnnouncement` | `RetargetAnnouncement(AnnouncementId, Audience)` | `AnnouncementRetargeted` |
| `WithdrawAnnouncement` | `WithdrawAnnouncement(AnnouncementId)` | `AnnouncementWithdrawn` |

- **The caller chooses the id.** Create it with `AnnouncementId.New()`. A retried `AuthorAnnouncement` is refused as already authored instead of authoring a second announcement.
- **How it is displayed never changes.** `Display` is set when the announcement is authored and no command amends it; withdraw the announcement and author another to change it.
- **No window means always.** Leave `ActiveFrom` and `ActiveUntil` at their default date (or send `ActiveFrom.Immediately` and `ActiveUntil.Indefinitely`) and the announcement is shown from the moment it is published, with no end. A start that is not before the end is refused.
- **Rejections are validation results.** A title is required and holds up to 100 characters, a body holds up to 2,000, an audience of tenants names at least one, an audience of a module or a plan names it, and `Display` and `Importance` are values that exist.
- **Only a standing announcement changes.** Amending, retargeting or withdrawing an announcement that was never authored, or that is withdrawn, is refused. Withdrawing twice is refused, so the withdrawal is published once.
- **Looking is not a fact.** Amending to the text and window it already has, and retargeting to the audience it already has, append nothing and succeed. The same tenants in another order are the same audience.

## Query

`Announcement.ForProduct(product)` is an observable query for the authoring screen. It returns the announcements of one product, withdrawn ones included and marked `Withdrawn`, the newest window first, and sends the list again each time one changes.

## The outbox

`MarqueeOutbox` is the one reactor behind all four events. It appends the public event to the **outbox** event sequence of the namespace the private event was appended in.

| Private event | Published as |
| --- | --- |
| `AnnouncementAuthored` | `AnnouncementPublished` |
| `AnnouncementAmended` | `AnnouncementRevised` |
| `AnnouncementRetargeted` | `AnnouncementAudienceChanged` |
| `AnnouncementWithdrawn` | `AnnouncementRetracted` |

- **One product-wide stream.** Every public event is appended under the product as its event source, so the announcements of a product arrive in the order they were published. The announcement is a field of the event.
- **Delivered once.** The publication carries an idempotency key derived from the reactor delivery. A redelivery or a replay appends nothing a second time, and every amendment is still forwarded.
- **A withdrawn announcement is never left standing.** The reactor reads what is recorded about the announcement. When it is handed a publication, an amendment or a new audience for an announcement whose withdrawal is already recorded, as after it was down, it publishes nothing for it; the retraction then follows. A product treats a retraction of an announcement it never received as a retraction in advance, see [Receiving](./receiving.md#a-retraction-always-wins).
- **A refusal is retried.** When the outbox refuses an event, the reactor throws `PublicationRefused`, so Chronicle delivers the private event again instead of losing the publication.
