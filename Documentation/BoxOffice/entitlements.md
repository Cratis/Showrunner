---
title: Receiving and entitlements
description: How a product adds Box Office, how the operator's decisions reach a tenant, and how the product asks what a tenant may use.
---

`Cratis.Showrunner.BoxOffice` is the part of Box Office a managed product hosts. The operator decides what a tenant may use; this library receives the decision, keeps it in the tenant's own namespace and answers what the tenant may do. It runs inside the product's Arc and Chronicle process, the same way [Usher](../Usher/backend.md) does.

## Add Box Office to a product

Reference the `Cratis.Showrunner.BoxOffice` package and call `AddBoxOffice` next to `AddCratis`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddBoxOffice(boxOffice => boxOffice.OperatorStores = ["Direct"]);
```

| Option | Default | Meaning |
| --- | --- | --- |
| `OperatorStores` | `["Direct"]` | The event stores of the operators whose decisions the product honors. |
| `PollingInterval` | one minute | How often Box Office looks for tenant namespaces it does not serve yet. |

The host fails to start when `OperatorStores` is empty or the interval is not more than zero. Arc discovers the commands, queries and command filter, and Chronicle the events, read models and reactors, from the referenced assembly; there is nothing else to register.

## How a decision reaches the tenant

The operator publishes its decisions as the public events in [Contracts](./contracts.md), under the organization the tenant belongs to. Box Office subscribes the application's event store to each operator store's outbox, so the events arrive in the application's inbox for that operator. A reactor on the inbox translates each decision about a tenant into a private fact and records it in the tenant's namespace, in one stream every tenant-wide fact shares.

The tenant is named by `TenantId`, and a tenant's id is the exact name of its Chronicle namespace, compared exactly: `Fjord-Logistics` is not `fjord-logistics`. Box Office never creates a namespace. A decision that names no tenant, a tenant with no namespace, or a name that two namespaces share is logged and recorded nowhere, so a decision can never land in another tenant's namespace.

Every decision is recorded with the time the operator decided it, and the newest decision wins whatever order decisions arrive in: a replay that delivers a revoke before the grant it followed still ends in the revoked state. A decision that the forwarder delivers twice is recorded once, because the record is keyed on the decision itself. Two separate decisions to the same effect, made at different times, are two decisions.

The decisions about the product as a whole, `OfferPublished` and `OfferWithdrawn`, are not about a tenant. They are recorded where the operator published them, and the `OnSale` query (the `Offer` read model) returns the offers that are not withdrawn.

The operator's `OrganizationId` is the event source of its events. Box Office records which organization a tenant is, so the product can answer under it, when the first decision for the tenant arrives.

## Ask what a tenant may use

Inject `IEntitlements` where a command or a query needs to know. It answers for the tenant of the current request:

| Question | Method |
| --- | --- |
| May the tenant change anything? | `IsActive()`, `WhyReadOnly()` |
| Does the tenant have a module? | `Has(module)`, `AccessTo(module)` |
| What may the tenant use of a meter? | `AllowanceFor(meter)` |
| How many seats may the tenant assign? | `SeatCapacity()` |
| Does a person hold a seat the tenant's seats cover? | `HasSeat(holder)` |

A module is `None`, `Preview`, `Trial` or `Full`. `Has` is true for a trial and for a full module, and not for a preview, which only lets the tenant look at a module it does not have. A meter the operator set no allowance for allows nothing.

Nothing that ends needs an event to end it. A trial, a module, a preview or seats granted until a day count until that day, and the answer changes the day after, computed from the clock when the question is asked. A trial's seats replace nothing the tenant bought: seats granted or bought replace the trial's, they do not add to them.

`IEntitlements` is for deciding whether to admit a command and for showing what the tenant has. The read is advisory: it can be as stale as the read model, so a command that must hold a rule under concurrency reads the facts it decides on itself, as [`AssignSeat`](./seats.md) does.

Box Office needs the operator's decisions to have arrived before it can answer; a tenant the operator has not decided anything about has no modules, no seats and no allowances, and is not read-only.
