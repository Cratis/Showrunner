---
title: The product side
description: Everything a managed product does for its operator in one call - create the tenants the operator registers and report them ready, ask before giving a seat, go read-only, and report who was active.
---

`Cratis.Showrunner.BoxOffice.Product` is what an Arc and Chronicle application adds to be a managed product of an operator such as Direct. One call gives it the whole of its side of the contract, and the application writes only what is its own: creating a tenant.

```csharp
builder.AddBoxOfficeProduct(options => options
    .ConfigureBoxOffice(boxOffice => boxOffice.OperatorStores = ["Direct"]));

builder.Services.AddScoped<IProvisionTenants, ProvisionTenants>();
```

`AddBoxOfficeProduct` adds Box Office itself, so a product calls it instead of `AddBoxOffice` and sets Box Office's options through `ConfigureBoxOffice`. The [Offering](./offering.md) stays as it was: the product calls `AddBoxOfficeOffering` for what it sells.

| The operator's need | What the product has |
| --- | --- |
| Create the tenant the operator registers, and say when it is ready | [`IProvisionTenants`](#creating-tenants), which the product implements |
| Ask before giving a seat | [`ISeatLicenses`](#asking-before-giving-a-seat) |
| Report a seat taken or released | Box Office's [seat commands](./seats.md#telling-the-operator), which report by themselves |
| Go read-only when the trial ends without seats, and open again when seats are granted | Box Office's [command filter](./suspension.md), and [`ITenantAccess`](#read-only-tenants) for everything that is not a command |
| Report who was active | [The sign-in callback](#reporting-activity) and [`IReportActivity`](#reporting-activity) |
| Hear that the trial ended, the seats changed or the organization is being deleted | Box Office [receives them](./entitlements.md); `IClosingTenants` is told of a closing tenant |

Nothing here calls the operator. The operator's decisions arrive as public events and are kept in the tenant's own namespace; every answer is read from there.

## Creating tenants

When a person registers an organization in the operator, it publishes `TenantRegistered`. The product creates the tenant and answers with `TenantProvisioned` under the same organization; the operator starts the trial's clock from that answer.

The product implements `IProvisionTenants` and registers it with its services. Box Office listens to the operator's inbox, calls it for each registration, and reports the answer:

```csharp
public class ProvisionTenants(ICommandPipeline commands) : IProvisionTenants
{
    public async Task<TenantProvisioning> Provision(TenantToProvision tenant, CancellationToken cancellationToken = default)
    {
        // Your own work: create the tenant's data and the creator's first role. Throw on a failure worth retrying.
        var created = await commands.Execute(new CreateTenant(tenant.Tenant, tenant.Name, tenant.Creator, tenant.CreatorRole));
        return created.IsSuccess
            ? TenantProvisioning.Provisioned
            : TenantProvisioning.Refused("The name is already used by another organization");
    }
}
```

- `Provision` must be idempotent. The registration can be delivered more than once, and creating a tenant that exists must change nothing and answer `Provisioned`.
- **Throw** for a failure that is worth another attempt: the registration is retried and nothing is reported.
- Answer **`Refused`** only for a refusal that retrying will not change. It is reported to the operator as `TenantNotProvisioned` with the reason, once, and the operator can show it.
- The answer is recorded once per tenant, however often the registration is delivered, and under the operator's organization (the event source of the registration; the id is not in the payload).
- When the tenant is created, Box Office also records which organization it is and who created it in the tenant's own namespace. Seat and activity reports need the organization, and a person without a seat is told who to ask.

A product that does not register `IProvisionTenants` creates no tenants and listens for nothing.

A product that wants to be told that an organization is being deleted also registers `IClosingTenants`; it is called for `TenantClosing` and the operator is told nothing back.

## Asking before giving a seat

Before the product gives someone a seat it asks whether they may take one:

```csharp
var license = await seatLicenses.For(tenant, SeatHolder.Of(person));
if (license.Status == SeatLicenseStatus.NoLicense)
{
    // Tell the person, and say who to ask: license.WhoToAsk.
}
```

| Status | Meaning |
| --- | --- |
| `Licensed` | The person already holds a seat the tenant's seats cover, or a seat is free for them to take. |
| `NoLicense` | The tenant has no seat for them: the seats are all held, it has none, it is read-only, or the person holds a seat the tenant's seats no longer cover. `WhoToAsk` holds the people who administer the tenant: the person who registered it. |
| `UnknownOrganization` | The product has no such tenant. |

The answer comes from the seats and trial the operator decided and from who holds a seat, read in the tenant's namespace. Nothing calls the operator, so the answer is as current as what the operator has published, and the pool cannot be overfilled: `AssignSeat` still guards the last seat under read protection.

## Read-only tenants

A tenant is read-only while the operator has it suspended, or when its trial ended and no seats were granted or bought. Box Office's [command filter](./suspension.md) refuses every command of a read-only tenant, with `TenantSuspended` or `TrialClosed` as the reason, and lets queries through. It needs no setup, and `[AllowedWhileSuspended]` lifts it for a command that must still run. When seats are granted or the operator reinstates the tenant, the next command goes through.

For work that is not a command, such as a job, a reactor or an endpoint, ask:

```csharp
if (await tenantAccess.IsReadOnly(tenant))
{
    return;
}
```

A tenant the product does not have is read-only, because nothing in it can change.

## Reporting activity

The operator shows when each seat holder was last seen. The product tells it with `SeatHolderActive`, which carries the tenant, the person and the day, and is published **at most once per person and day**: seeing someone again the same day is not a fact. The reporter remembers who it reported today, and the event log keeps its own key of the person, tenant and day, so a restart or a second replica does not report the day again.

### The sign-in callback

AuthProxy can call the product when a person signs in, and when a returning person's session makes its first request. `AddBoxOfficeProduct` receives that call by itself, with no endpoint to map: it answers `POST /.cratis/showrunner/sign-in`, before the rest of the pipeline, and reports the person as active.

Declare the callback on the product's service in AuthProxy's configuration, as its `SignInCallback` (it is off until declared):

```json
{
  "Cratis": {
    "AuthProxy": {
      "Services": {
        "core": {
          "Backend": { "BaseUrl": "http://core:8080/" },
          "SignInCallback": {
            "Path": "/.cratis/showrunner/sign-in",
            "Interval": "12:00:00",
            "Secret": "<from the secret store>"
          }
        }
      }
    }
  }
}
```

Give the product the same secret, from its secret store or environment and never a committed file:

```csharp
builder.AddBoxOfficeProduct(options => options.SignInSecret = builder.Configuration["Showrunner:SignInSecret"]!);
```

What the product checks on a call:

- The `x-cratis-sign-in-callback` marker header, with the kind `SignedIn` or `Returned`. AuthProxy strips this header from every request that comes through it, so a browser cannot send it.
- The `x-cratis-sign-in-callback-secret` header, compared in constant time, when `SignInSecret` is set. Set it: without it the product relies on its network alone.
- The person in the body must be the person AuthProxy presents on the call (`x-ms-client-principal-id`), so a call can only report the person it is made for.
- The tenant comes from the body, or from `x-cratis-tenant-id` when the body has none. A call with no tenant reports nothing. A header and a body that disagree are refused.

| Answer | When |
| --- | --- |
| `204` | The call was accepted, whether or not it reported anything: the day was already reported, the tenant is not known, or the tenant does not know its organization yet. |
| `400` | The body is missing, too long, not a notification, names someone other than the person presented, or the tenant differs between the body and the header. |
| `401` | The marker is missing or is not a kind, or the secret is wrong. |

`BoxOfficeProductOptions.SignInPath` changes the path and `ReportSignIns = false` turns the callback off.

### Reporting activity yourself

For activity AuthProxy cannot see, such as a person acting through a token, ask for the same report:

```csharp
await reportActivity.Report(tenant, person);
```

`IReportActivity.Report` is idempotent per person and day, and answers with an `ActivityOutcome`: `Reported`, `AlreadyReported`, `UnknownTenant` or `OrganizationNotKnown`.

## The contract this adds

Two new public events join [the contracts](./contracts.md). Nothing existing changes.

| Event | Event source | Payload |
| --- | --- | --- |
| `SeatHolderActive` | `OrganizationId` | `TenantId`; `Holder: Person`; `Day: ActivityDay` |
| `TenantNotProvisioned` | `OrganizationId` | `TenantId`; `Reason: NotProvisionedReason` |
