---
title: Contracts
description: The public events Marquee exchanges between the people publishing announcements and the products showing them, their payloads, and how an audience is resolved.
---

`Cratis.Showrunner.Marquee.Contracts` holds the public events a product receives announcements through. The package references only `Cratis.Chronicle`, so a publisher and a product can both depend on it without depending on the rest of Showrunner.

## Rules every contract follows

- **An announcement belongs to a product, not to a tenant.** Announcements of a product share one product-wide stream, so `AnnouncementId` is a payload field, not the event source id. This is the one exception to carrying a `TenantId` in every payload: tenant ids appear only inside an `Audience` that names tenants.
- **Every event is `[Public]`, and none names an origin store.** There is no `[EventStore]` attribute on any contract or on the assembly, because store names vary by deployment. Each side subscribes to the other explicitly.
- **Events are named for what happened**, and no name is shared with a private event.
- **Nothing is nullable.** A value that does not apply is empty: `ModuleKey.NotSet`, `PlanId.NotSet`, no tenants. An announcement with no end uses `ActiveUntil.Indefinitely`.
- **The window is exact UTC times.** `ActiveFrom` and `ActiveUntil` are `DateTimeOffset` concepts, not whole days, so a maintenance window can be announced to the minute.

Namespaces are relative to `Cratis.Showrunner.Marquee.Contracts`. `TenantId` sits in the root namespace.

## Events

| Event | Namespace | Payload |
| --- | --- | --- |
| `AnnouncementPublished` | `Announcements` | `AnnouncementId`; `Product`; `Audience`; `Title: AnnouncementTitle`; `Body: AnnouncementBody`; `Display`; `Importance`; `ActiveFrom`; `ActiveUntil` |
| `AnnouncementRevised` | `Announcements` | `AnnouncementId`; `Product`; `Title`; `Body`; `Importance`; `ActiveFrom`; `ActiveUntil` |
| `AnnouncementAudienceChanged` | `Announcements` | `AnnouncementId`; `Product`; `Audience` |
| `AnnouncementRetracted` | `Announcements` | `AnnouncementId`; `Product` |

`AnnouncementRevised` states the full new text and window, so a reader never combines it with earlier events. It does not change the audience or the display; `AnnouncementAudienceChanged` changes the audience.

## Shared types

| Type | Namespace | Shape |
| --- | --- | --- |
| `AnnouncementId` | `Announcements` | `ConceptAs<Guid>` |
| `Product` | `Announcements` | Text; the product an announcement is for |
| `AnnouncementTitle`, `AnnouncementBody` | `Announcements` | Text; the body is Markdown |
| `ActiveFrom`, `ActiveUntil` | `Announcements` | `ConceptAs<DateTimeOffset>` |
| `Display` | `Announcements` | `Banner = 0`, `DialogOnce = 1`, `DialogEverySession = 2` |
| `Importance` | `Announcements` | `Information = 0`, `Warning = 1`, `Critical = 2` |
| `Audience` | `Audiences` | `Kind: AudienceKind`; `TenantIds: IEnumerable<TenantId>`; `Module: ModuleKey`; `Plan: PlanId` |
| `AudienceKind` | `Audiences` | `Everyone = 0`, `Tenants = 1`, `HoldersOfModule = 2`, `OnPlan = 3` |
| `ModuleKey`, `PlanId` | `Audiences` | Text |
| `TenantId` | root | The tenant's namespace name |

## Audiences

Build an audience with its factory:

```csharp
Audience.Everyone();
Audience.Tenants("acme", "globex");
Audience.HoldersOfModule("reports");
Audience.OnPlan("pro");
```

`Kind` says which of the other values applies. The rest are empty, so the payload has no nulls.

### Plugging in who holds a module or is on a plan

`Everyone` and `Tenants` are decided from the audience itself. `HoldersOfModule` and `OnPlan` are not: whether a tenant holds a module or is on a plan belongs to whatever sells the product. Marquee therefore depends on one abstraction, `IAudienceEntitlements`, and on nothing that provides it:

```csharp
public interface IAudienceEntitlements
{
    ValueTask<bool> HoldsModule(TenantId tenantId, ModuleKey module);
    ValueTask<bool> IsOnPlan(TenantId tenantId, PlanId plan);
}
```

Ask an audience whether it reaches a tenant with `Includes`:

```csharp
var reaches = await announcement.Audience.Includes(tenantId, entitlements);
```

A host that has an entitlement source, such as Box Office or its own tables, implements the interface and registers it. A host without one passes `NoAudienceEntitlements.Instance`: it still gets `Everyone` and `Tenants` audiences, and module and plan audiences reach nobody.

## Released generations

`EventSchemas.snapshot.json` records the schema of every event. A specification compares the code with it, so a changed schema fails the build until the snapshot is regenerated on purpose:

```shell
MARQUEE_UPDATE_EVENT_SCHEMA_SNAPSHOT=1 dotnet test Source/DotNET/Marquee.Contracts/Marquee.Contracts.csproj
```

`ReleasedEventSchemas.json` lists the generations that have shipped. A listed generation must stay in the snapshot and in code, and its schema can never change: a released event needs a new generation and a migration instead.
