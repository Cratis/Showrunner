---
title: "Box Office: integrate a managed product"
description: Add Box Office to an Arc and Chronicle product to provision tenants for its operator, check seat and read-only access, and report activity.
---

`Cratis.Showrunner.BoxOffice.Product` connects an Arc and Chronicle application to the operator that manages its tenants and licensing. Use it to receive tenant registrations, report when a tenant is ready, and check access from the licensing decisions already received by the product.

In a host already configured with Arc and Chronicle, call `AddBoxOfficeProduct` and register your tenant-creation implementation:

```csharp
builder.AddBoxOfficeProduct(options => options
    .ConfigureBoxOffice(boxOffice => boxOffice.OperatorStores = ["Direct"]));

builder.Services.AddScoped<IProvisionTenants, ProvisionTenants>();
```

`OperatorStores` names the operator's Chronicle event stores. `AddBoxOfficeProduct` adds Box Office itself, so use it instead of `AddBoxOffice` and pass Box Office's options through `ConfigureBoxOffice`. Your product still declares what it sells separately with `AddBoxOfficeOffering`; see [Offering](./offering.md).

| The operator's need | What the product has |
| --- | --- |
| Create the tenant the operator registers, and say when it is ready | [`IProvisionTenants`](#creating-tenants), which the product implements |
| Ask before giving a seat | [`ISeatLicenses`](#asking-before-giving-a-seat) |
| Report a seat taken or released | Box Office's [seat commands](./seats.md#telling-the-operator), which report by themselves |
| Go read-only when the trial ends without seats, and open again when seats are granted | Box Office's [command filter](./suspension.md), and [`ITenantAccess`](#read-only-tenants) for everything that is not a command |
| Report who was active | [The sign-in callback](#reporting-activity) and [`IReportActivity`](#reporting-activity) |
| Hear that the trial ended, the seats changed or the organization is being deleted | Box Office [receives them](./entitlements.md); `IClosingTenants` is told of a closing tenant |

These access checks do not call the operator. Its decisions arrive as public events and are kept in the tenant's own Chronicle namespace. The product reads them there and sends reports back through its outbox.

## Creating tenants

`TenantRegistered` asks the product to create a tenant. Box Office calls your `IProvisionTenants` implementation and reports a successful result as `TenantProvisioned`, using the registration's organization as the event source. This separates a request to create a tenant from confirmation that it is ready.

Register `IProvisionTenants` with your services as shown above. Your implementation supplies the application's own creation logic; `CreateTenant` below represents that application-specific command:

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

The registration can be delivered more than once, so `Provision` must be idempotent: creating a tenant that already exists must change nothing and return `Provisioned`. Box Office records the answer once per tenant under the registration's organization. That organization ID is the event source, not a field in the registration payload.

Throw for a failure worth another attempt. The registration is retried without reporting a provisioning answer. Return `Refused` only when retrying will not change the outcome, such as the name conflict in this example. Box Office reports that refusal once as `TenantNotProvisioned`, including the reason.

After successful provisioning, Box Office records the tenant's organization and creator in the tenant's own namespace. Seat and activity reports use that organization; the seat-license check uses the creator to tell a person who to ask for a seat.

Without an `IProvisionTenants` registration, Box Office installs no provisioning subscriptions and creates no tenants. If your product also needs to act when a tenant is closing, register `IClosingTenants`. Box Office calls it for `TenantClosing` and sends no acknowledgement to the operator.

## Asking before giving a seat

Use `ISeatLicenses` to check whether a person may take a seat before offering them one. The following excerpt assumes you have the tenant, person and an injected `ISeatLicenses` named `seatLicenses`:

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
| `NoLicense` | The tenant has no seat for them: the seats are all held, it has none, it is read-only, or the person holds a seat the tenant's seats no longer cover. `WhoToAsk` contains the tenant's creator when known. |
| `UnknownOrganization` | The product has no such tenant. |

This is a local check against the operator's decisions received so far and the tenant's seat roster. It does not reserve a seat. Use Box Office's [seat commands](./seats.md) to assign or release one: `AssignSeat` guards the last seat under read protection, so a competing assignment cannot overfill the pool.

## Read-only tenants

A tenant is read-only while the operator has it suspended, or when its trial ends without seats. Box Office's [command filter](./suspension.md) refuses commands with `TenantSuspended` or `TrialClosed` and leaves queries available. `AddBoxOfficeProduct` includes this filter; `[AllowedWhileSuspended]` exempts a command from the read-only check.

A seat grant removes the trial-closed restriction, but a separate suspension still requires reinstatement. Once the relevant decision reaches the tenant, subsequent commands use it.

Jobs, reactors and endpoints outside the command pipeline need their own check. Inject `ITenantAccess` as `tenantAccess` and ask before changing tenant data:

```csharp
if (await tenantAccess.IsReadOnly(tenant))
{
    return;
}
```

`IsReadOnly` also returns `true` for a tenant the product does not know, so this check does not admit work for an unknown tenant.

## Reporting activity

Report activity so the operator can track when a person was last seen in a tenant. Box Office publishes `SeatHolderActive` with the tenant, person and UTC day, at most once per person, tenant and day. It remembers today's reports in memory and uses the same identity as an event-log key, preventing duplicate daily reports after a restart or from another replica.

### The sign-in callback

AuthProxy can notify the product when a person signs in or returns with an existing session. Return notifications are limited by the configured interval. `AddBoxOfficeProduct` receives the callback before the rest of the pipeline at `POST /.cratis/showrunner/sign-in` and reports the person as active; you do not map an endpoint.

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

Set `SignInSecret` in your earlier `AddBoxOfficeProduct` call, shown separately below. Use the same secret as AuthProxy, read from a secret store or environment rather than a committed file:

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

`IReportActivity.Report` is idempotent per person, tenant and UTC day. Its `ActivityOutcome` is `Reported` or `AlreadyReported` when the daily report is recorded, `UnknownTenant` when the product cannot find the tenant, or `OrganizationNotKnown` when the tenant has not yet recorded its organization. The last two outcomes publish no activity report.

## The contract this adds

Provisioning refusals and daily activity use these public events. Both identify the tenant in the payload and use its organization as the event source:

| Event | Event source | Payload |
| --- | --- | --- |
| `SeatHolderActive` | `OrganizationId` | `TenantId`; `Holder: Person`; `Day: ActivityDay` |
| `TenantNotProvisioned` | `OrganizationId` | `TenantId`; `Reason: NotProvisionedReason` |

Continue with [the contracts reference](./contracts.md) for the full exchange between operator and product, or [seat licensing](./seats.md) to connect the license check to seat assignment.
