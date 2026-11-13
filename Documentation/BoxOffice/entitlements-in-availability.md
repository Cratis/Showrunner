---
title: Use entitlements in availability conditions
description: Let a Cratis Choreography availability gate read a module's level, whether the tenant is active and how many seats it has, as the host-provided value entitlements, and see how Box Office modules relate to GuestList audiences.
---

Use this in a product that evaluates availability with [Cratis Choreography](https://github.com/Cratis/Choreography) and wants a gate to depend on what the tenant bought. A gate can read Box Office entitlements through `$context`; Box Office itself is unchanged and does not know the engine.

## Add it

1. Reference `Cratis.Showrunner.BoxOffice.Product.Availability` next to `Cratis.Showrunner.BoxOffice.Product`. It is a separate package so a product that does not use Choreography takes no dependency on it.
2. Register it next to `AddBoxOfficeProduct`:

```csharp
builder.Services.AddBoxOfficeProduct();
builder.Services.AddBoxOfficeEntitlementsAvailability();
```

`AddBoxOfficeEntitlementsAvailability` registers `EntitlementsHostContextValues` as the engine's `IHostContextValues`, unless the host registered its own. A host that supplies other values as well implements `IHostContextValues` itself and answers `EntitlementsHostContextValues.Name` (`entitlements`) by calling `EntitlementsHostContextValues.Get`; every other name it answers itself.

## Declare the value in the model

The model declares the value as supplied by the host, then reads it in a condition:

```screenplay
provide entitlements Entitlements from host
```

A gate reads it as `$context.entitlements` followed by a path. Paths are not case-sensitive.

| Path | Type | Meaning |
| --- | --- | --- |
| `entitlements.active` | boolean | The tenant can change things: it is not suspended and its trial has not closed without seats. |
| `entitlements.readOnlyReason` | text | `None`, `TenantSuspended` or `TrialClosed`. |
| `entitlements.seats` | number | How many seats the tenant may use. |
| `entitlements.modules.<module>.level` | text | `None`, `Preview`, `Trial` or `Full`. |
| `entitlements.modules.<module>.usable` | boolean | The tenant may use the module: `Trial` or `Full`. A preview is not usable. |

`<module>` is the module's id as the product declared it in its offering. A module the product declares as included is `Full` for every tenant. A module the product does not declare is absent, which the engine treats as unknown, and unknown denies.

Because the engine compares a number against a number and text against text, write the level as text, for example `level` equal to `"Full"`, and capacity as a number, for example `seats` at least `5`.

## What is read, and when

- **Lazily.** Nothing is read until a gate reads `$context.entitlements`. A request whose gates never read it does not touch Box Office.
- **Once per request.** The engine keeps the value for the rest of the request, so every gate sees the same answer. Nothing is kept between requests.
- **For the tenant of the request.** The value is read from the scope of the current HTTP request, so it answers for the tenant that request belongs to. Reading it outside a request fails loudly; it is a failure, not a denial.

The answers are those of `IEntitlements`, which reads what the operator decided when asked, so a trial or module that ended stops counting on its last day without an event.

## How modules and audiences relate

Box Office modules and GuestList audiences stay separate. A gate can use both, and neither is merged into the other.

| | Box Office modules | GuestList audiences |
| --- | --- | --- |
| Question | What did the tenant buy or try? | Who is this part of the product shown to? |
| Kind | Commercial | Non-commercial |
| Held by | The tenant, in its own namespace | The tenant, a user or the whole system |
| Decided by | The operator's offer, a trial or a purchase | The operator granting or withdrawing an audience |
| Read in a gate | `$context.entitlements...` | `available for` and `$context.audiences` |

Use a module when access follows a purchase, and an audience when it follows a rollout, such as a preview group or an internal test. A gate that needs both says both: the audience decides whether the tenant or person is let in, and the module condition decides whether what they bought covers it.

An audience does not grant a module and a module does not grant an audience. Module gating by namespace, described in [Module gating](gating.md), is unchanged and still enforced on commands and queries whatever a gate says.

See also [Feed the availability engine](../GuestList/feed-the-availability-engine.md) for the audience side.
