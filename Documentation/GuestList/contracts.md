---
title: GuestList contracts
description: The public events an operator publishes to grant and withdraw an audience, the types they carry and how their schema is guarded.
---

`Cratis.Showrunner.GuestList.Contracts` holds the events an operator and a product exchange. It has no behavior and depends only on Chronicle. The events are `[Public]` and name no origin store, because they are shared by operator stores whose names vary by deployment; the product subscribes to the operator stores explicitly, see [Receive audiences in a product](./receive-audiences-in-a-product.md).

## Events

All types are in `Cratis.Showrunner.GuestList.Contracts.Audiences`.

| Event | Fields | Meaning |
| --- | --- | --- |
| `AudienceGranted` | `Product`; `Level: AudienceLevel`; `TenantId`; `UserId`; `Audience: AudienceName` | An operator grants an audience at a level |
| `AudienceWithdrawn` | `Product`; `Level: AudienceLevel`; `TenantId`; `UserId`; `Audience: AudienceName` | An operator withdraws an audience from a level |

Which of `TenantId` and `UserId` are filled depends on the level. The others are empty, so the payload has no nulls.

| `Level` | Value | `TenantId` | `UserId` |
| --- | --- | --- | --- |
| `Tenant` | 0 | the tenant | empty |
| `User` | 1 | the user's tenant | the user |
| `System` | 2 | empty | empty |

The meaning is the same for both events:

- A grant or a withdrawal affects its own level only. Withdrawing `Plus` from a user leaves the tenant's and the system's grants of `Plus` in place.
- The newest decision about the same audience at the same level wins, judged by when the operator decided, not by when the event arrived. An older grant that arrives after a newer withdrawal does not bring the audience back.
- A decision that arrives twice is recorded once.
- An audience name the product does not declare is kept like any other. It never matches a gate and shows up in the undeclared grants report.

The events carry no event source of their own. The operator appends them under the product's name, so all decisions of a product share one stream and arrive in the order they were taken.

## Shared types

| Type | Shape |
| --- | --- |
| `Product` | Text; the product the decision is for |
| `TenantId` | Text; the name of the tenant's Chronicle namespace in the product |
| `UserId` | Text; the user as the product's identity provider names them |
| `AudienceName` | Text; starts with a letter, then letters, digits and underscores, up to 64 characters, case-sensitive |
| `AudienceLevel` | `Tenant = 0`, `User = 1`, `System = 2` |

## Audience is also a routing label

Chronicle has its own routing option called the audience: a label on a subscription and on an event that decides which subscriber receives it, see [Audience routing](../BoxOffice/audience-routing.md). It is separate from the `Audience` field of these events, which is the name of the audience being granted. GuestList uses the routing audience to address a decision to its product, with the product's name as the label. The field never takes part in routing.

## Released generations

`EventSchemas.snapshot.json` records the schema of every event. A specification compares the code with it, so a changed schema fails the build until the snapshot is regenerated on purpose:

```shell
GUESTLIST_UPDATE_EVENT_SCHEMA_SNAPSHOT=1 dotnet test Source/DotNET/GuestList.Contracts/GuestList.Contracts.csproj
```

`ReleasedEventSchemas.json` lists the generations that have shipped. A listed generation must stay in the snapshot and in code, and its schema can never change: a released event needs a new generation and a migration instead.
