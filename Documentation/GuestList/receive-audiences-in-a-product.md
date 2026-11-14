---
title: Receive audiences in a product
description: Add GuestList to a product so it records the audiences an operator grants and answers which audiences a tenant, a user or the system is in.
---

Use this in the application that is the product. It needs a Chronicle event store and Arc.

## Add GuestList

1. Reference `Cratis.Showrunner.GuestList`. Arc discovers its queries and Chronicle its events and reducers.
2. Add it next to Chronicle and name the product as the operator's decisions name it:

```csharp
builder.AddCratisChronicle(options => options.EventStore = "Ledger");
builder.AddGuestList(guestList =>
{
    guestList.Product = "ledger";
    guestList.DeclaredAudiences = ["Plus", "Preview", "Internal"];
});
```

3. Tell GuestList which operator store the decisions come from. The default is `Direct`, which is only a store name: nothing calls Direct.

| Option | Default | What it does |
| --- | --- | --- |
| `Product` | none, required | The product this application is |
| `OperatorStores` | `Direct` | The event stores whose outbox the decisions come from |
| `SubscribeForProduct` | `true` | Subscribe for this product's routing audience only. Turn off only when the operator does not address decisions to products |
| `DeclaredAudiences` | unset | The audiences the model declares. Unset reports nothing as undeclared |
| `PollingInterval` | one minute | How often GuestList makes sure it is subscribed |

GuestList subscribes the product's store to each operator store's outbox for `AudienceGranted` and `AudienceWithdrawn`, and handles them in the inbox of that store. The subscription id is `guestlist-<operator store>`, so it does not collide with what Marquee or Box Office subscribe to the same store.

A tenant is a Chronicle namespace of the product, named by its `TenantId`. Decisions about a tenant are recorded in that namespace. Decisions about the system are recorded in the default namespace.

## Read what was recorded

Inject `IGuestList` to build the audiences of a request:

```csharp
IReadOnlySet<AudienceName> audiences = await guestList.For(tenant, user);
```

| Method | Answers |
| --- | --- |
| `ForTenant(tenant)` | the audiences granted to the tenant as a whole |
| `ForUser(tenant, user)` | the audiences granted to one user |
| `ForSystem()` | the audiences granted to every system |
| `ForNamedSystem(client)` | the audiences granted to one named system only, by its client id |
| `ForSystemCaller(client)` | what a request made by that system gets: the ones granted to every system plus its own; with no client id, the ones granted to every system only |
| `For(tenant, user)` | the three added together; pass `UserId.NotSet` for a caller that is not a person |
| `Granted(tenant)` | every grant that applies to the tenant, with its level |

Arc also exposes model-bound queries for the frontend. They answer for the tenant of the request.

| Read model | Query | Returns |
| --- | --- | --- |
| `GrantedAudience` | `ForTenant` | the tenant's audiences |
| `GrantedAudience` | `ForUser(user)` | one user's audiences |
| `GrantedAudience` | `ForSystem` | the system's audiences |
| `UndeclaredGrant` | `Undeclared` | grants that match no declared audience |

Every query requires an authenticated caller.

## What it does when things go wrong

- A decision for another product is ignored.
- A decision that names no audience, or no tenant or user its level needs, is logged and not recorded.
- A decision for a tenant namespace the product does not have is logged and not recorded.
- When the event log refuses a record, the inbox throws, so Chronicle delivers it again.
