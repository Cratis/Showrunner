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
| `DeclaredAudiences` | empty | For each product, the audiences its application declares; used by the undeclared grants report when the product has not reported its own declaration |
| `PrimaryEnvironment` | `Production` | The environment whose reported declaration counts when a product reports from more than one |

## Commands

| Command | Fields | Does |
| --- | --- | --- |
| `GrantAudience` | `Product`, `Level`, `TenantId`, `UserId`, `Audience`, `ClientId`, `Kind` | Grants an audience at a level |
| `WithdrawAudience` | `Product`, `Level`, `TenantId`, `UserId`, `Audience`, `ClientId`, `Kind` | Withdraws it from that level only |

Each level names exactly what it needs: a tenant decision a tenant and no user, a user decision a tenant and a user, a system decision neither, and a named-system decision (`NamedSystem`) a client id and a kind (`Mcp`, `Api` or `Other`) and no tenant or user. `ClientId` and `Kind` are optional for the other levels, so a caller written before named systems keeps working. Anything else is refused with a validation message, as are an empty product, an invalid audience name and a tenant the `IKnownTenants` does not know. Granting an audience that is held, or withdrawing one that is not, succeeds and records nothing, so a retried command is harmless.

```csharp
await commands.Execute(new GrantAudience("ledger", AudienceLevel.User, "acme", "alice", "Plus"));
```

### Grant to one system

`System` grants an audience to every system that calls the product. To grant one to a single system, use `NamedSystem` with the system's client id and kind:

```csharp
await commands.Execute(new GrantAudience("ledger", AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, "Plus", "crm-sync", SystemKind.Mcp));
```

A system is who its client id says it is, so granting again with another kind records nothing. Withdrawing from a named system removes that system's grant only; the grants of `System` and of other named systems stay. The client ids a product knows are in the `ProductSystems` query, see [Systems that call a product](#systems-that-call-a-product).

## Queries

| Read model | Query | Returns |
| --- | --- | --- |
| `ProductGuestList` | `ForProduct(product)` | every grant held for the product, observable |
| `UndeclaredAudienceGrant` | `ForProduct(product)` | the grants whose audience the product does not declare, observable; empty when no audiences are declared for the product. What the product reported for the primary environment beats `DeclaredAudiences`, which stays as the fallback for a product that has not reported |
| `ProductAvailability` | `ForProduct(product)` | what the product last declared in each environment it reports from, and what each report changed, observable |
| `ProductSystems` | `ForProduct(product)` | the systems the product last said call it, in each environment it reports from, with their client id, kind and name, observable |
| `AudienceDelivery` | `ForProduct(product)` | for each environment, the newest decision the product told the operator it recorded, observable |

The last three are built from what the product reports, see [Report what the model declares](./report-what-the-model-declares.md). Until a product reports, they hold nothing about it, and nothing is claimed.

### Systems that call a product

`ProductSystems` lists the client ids an operator can pick from when granting to one system. A product reports them as `SystemsDeclared`, see [Report what the model declares](./report-what-the-model-declares.md#report-the-systems-that-call-the-product). Each report replaces the earlier list of its environment. A grant to a client id the product does not list is kept: the list says who can be picked, not who holds what. `ProductGuestList` and `UndeclaredAudienceGrant` carry `ClientId` and `Kind` for named-system grants.

## What is published

The commands record `AudienceGrantDecided` and `AudienceWithdrawalDecided` in the stream of the product. One reactor, `GuestListOutbox`, publishes each as the matching public event, see [Contracts](./contracts.md), keeping the time the operator decided and appending with an idempotency key so a redelivery or a replay publishes nothing twice. Nothing in the authoring side calls Direct or any other service.
