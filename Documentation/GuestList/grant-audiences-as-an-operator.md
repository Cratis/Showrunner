---
title: Grant audiences as an operator
description: Host the GuestList authoring backend in your own application to grant and withdraw audiences for your products, without Direct.
---

The operator side is the package `Cratis.Showrunner.GuestList.Authoring`. Any Arc application with a Chronicle event store can host it, and Direct is not required.

## Host the authoring backend

1. Reference `Cratis.Showrunner.GuestList.Authoring`.
2. Register the policy that admits who may decide. Every command and query requires `GuestListAuthoring.Policy`, and a host that does not register it cannot use them:

```csharp
builder.Services.AddArcAuthorizationPolicy<OperatorsOnly>(GuestListAuthoring.Policy);
builder.Services.AddGuestListAuthoring();
```

3. Optionally register an `IKnownTenants` that knows the tenants of your products, such as one backed by Box Office. Without it any named tenant is accepted and the product refuses a tenant it does not have.
4. Name your event store in each product's `OperatorStores`, see [Receive audiences in a product](./receive-audiences-in-a-product.md).

| Option | Default | What it does |
| --- | --- | --- |
| `AddressToProducts` | `true` | Address each decision to its product with Chronicle's routing audience, so a product that subscribes for itself receives only its own |
| `DeclaredAudiences` | empty | For each product, the audiences its application declares; used by the undeclared grants report |

## Commands

| Command | Fields | Does |
| --- | --- | --- |
| `GrantAudience` | `Product`, `Level`, `TenantId`, `UserId`, `Audience` | Grants an audience at a level |
| `WithdrawAudience` | `Product`, `Level`, `TenantId`, `UserId`, `Audience` | Withdraws it from that level only |

Each level names exactly what it needs: a tenant decision a tenant and no user, a user decision a tenant and a user, a system decision neither. Anything else is refused with a validation message, as are an empty product, an invalid audience name and a tenant the `IKnownTenants` does not know. Granting an audience that is held, or withdrawing one that is not, succeeds and records nothing, so a retried command is harmless.

```csharp
await commands.Execute(new GrantAudience("ledger", AudienceLevel.User, "acme", "alice", "Plus"));
```

## Queries

| Read model | Query | Returns |
| --- | --- | --- |
| `ProductGuestList` | `ForProduct(product)` | every grant held for the product, observable |
| `UndeclaredAudienceGrant` | `ForProduct(product)` | the grants whose audience the product does not declare, observable; empty when no audiences are declared for the product |

## What is published

The commands record `AudienceGrantDecided` and `AudienceWithdrawalDecided` in the stream of the product. One reactor, `GuestListOutbox`, publishes each as the matching public event, see [Contracts](./contracts.md), keeping the time the operator decided and appending with an idempotency key so a redelivery or a replay publishes nothing twice. Nothing in the authoring side calls Direct or any other service.
