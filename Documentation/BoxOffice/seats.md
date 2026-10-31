---
title: Seat licensing
description: How a product assigns and releases seats, what keeps two assignments from overfilling the pool, and what happens when the pool shrinks.
---

Who holds a seat is the product's decision. How many seats exist is the operator's. Box Office keeps the two apart: the operator sets the capacity, the product assigns the seats within it.

## Assigning and releasing seats

`AssignSeat(Holder)` gives a person a seat and `ReleaseSeat(Holder)` takes it back. They record `SeatAssigned` and `SeatUnassigned` in the tenant's stream.

- Assigning a seat to someone who already holds one appends nothing, and neither does releasing a seat nobody holds.
- The pool is full when the seats held reach the capacity. `AssignSeat` is then refused with a validation message, and so is an assignment once the tenant's seats have ended.
- Every assignment and release is recorded in one stream, and `AssignSeat` reads the roster under Arc read protection. Two assignments that race for the last seat both read the same roster; the second to append finds the roster changed and is refused as a concurrency violation, so the pool cannot be overfilled. The client resubmits, and the resubmission sees the real roster.

Assigning seats is for the product's maintainers and the policy is the product's. Both commands and the `CurrentRoster` query require the authorization policy `BoxOffice.ManageSeats`, which Box Office does not define. The product registers it, with Arc's policy registration or with ASP.NET Core; until it does, ASP.NET Core cannot find the policy and those calls fail rather than succeed, so nobody can assign a seat by accident:

```csharp
builder.Services.AddArcAuthorizationPolicy<ProductMaintainers>(BoxOfficePolicies.ManageSeats);
```

## Requiring a seat

The product says which commands need a seat. Mark them `RequiresSeat`:

```csharp
[Command]
[RequiresSeat]
public record ChangeSettings(...)
```

A command with the marker from a user who holds no seat the tenant's seats cover is refused with `SeatRequired`. Who the user is comes from the signed-in identity: the `sub` claim and Arc's identity provider claim, the same pair a seat is assigned to. `SeatRequired` still applies to a command that is also `AllowedWhileSuspended`.

## When the pool shrinks

When the operator lowers the capacity below the seats held, the seats are not released. The holders who were assigned first stay licensed up to the capacity, oldest first, and the rest keep their seat but are not licensed: `RequiresSeat` refuses them. The `Mine` query returns a `SeatStanding` that tells each user where they stand so the frontend can tell them why:

| Status | Meaning |
| --- | --- |
| `NoSeat` | The user holds no seat. |
| `Licensed` | The user holds a seat the tenant's seats cover. |
| `OverCapacity` | The user holds a seat, but the tenant has fewer seats than are held and theirs is not covered. |

It also carries the capacity, the number held and why the tenant is read-only, when it is.

## Telling the operator

Each assignment and release is reported to the operator as `SeatTaken` and `SeatReleased` in the outbox, under the operator's organization for the tenant. The report is keyed on the delivery of the fact, so a repeated delivery is reported once. A seat that is assigned before Box Office knows which organization the tenant is fails and is retried until the operator's first decision has arrived.
