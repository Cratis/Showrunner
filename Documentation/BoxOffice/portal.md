---
title: The customer portal
description: How a product links its tenants to the operator's customer portal - the options, the signed link, what the portal can rely on, and how to verify a link.
---

A tenant buys seats, tries a module and changes plan at the operator's customer portal, not in the product. Box Office gives the product one thing for it: a signed link that opens the portal for the right tenant, so the portal does not have to ask who is calling and the tenant cannot be changed by editing the address.

## Configure the portal

The portal is optional. A product that sets neither option has no customer portal: the frontend does not offer to buy, and asking for a link fails with `CustomerPortalNotConfigured`.

| Option | Default | Meaning |
| --- | --- | --- |
| `CustomerPortalUrl` | none | The address of the operator's customer portal. |
| `CustomerPortalSigningKey` | none | The key the product and the operator share. Keep it in a secret store, never in source. |
| `CustomerPortalLinkLifetime` | 15 minutes | How long a link works. More than zero and at most a day. |

```csharp
builder.AddBoxOffice(boxOffice =>
{
    boxOffice.CustomerPortalUrl = new("https://portal.example.com/box-office");
    boxOffice.CustomerPortalSigningKey = builder.Configuration["BoxOffice:PortalSigningKey"];
});
```

The host fails to start when only one of the address and the key is set, so a half-configured portal cannot sign links nobody can verify.

## What a link carries

`PortalLink.LinkToPortal` is a query the frontend calls through `LinkToPortal` in [`@cratis/showrunner.boxoffice`](./frontend-proxies.md), with what the user wants to do and, when they looked at one, the offer. Any signed-in user may ask for a link. The link is the portal's address with these parameters:

| Parameter | Meaning |
| --- | --- |
| `tenant` | The tenant, which is the name of its Chronicle namespace. |
| `intent` | `buy`, `try` or `changeplan`. |
| `offer` | The offer the user looked at; left out when there was none. |
| `person` | The signed-in user as `provider:subject`, so the portal can say who is buying; left out when the user is not known. |
| `expires` | When the link stops working, in seconds since 1970-01-01 UTC. |
| `signature` | An HMAC-SHA256 over the other parameters, as base64url without padding. |

The tenant comes from the application's event store, never from the request, so a user cannot ask for a link for another tenant.

## Verify a link in the portal

The portal makes the same signature with the shared key and refuses the link when it differs, or when `expires` has passed. The text that is signed is the five values joined with a line feed, in this order, empty when a value is left out:

```text
tenant
intent
offer
person
expires
```

`SignedCustomerPortal.Canonical` builds that text, for a portal written in .NET. Compare signatures in constant time. A link is a bearer credential for its lifetime, so keep the lifetime short and do not log full links.

Which tenant, what the user intends and who they are is all the link says. What the portal then sells and how it takes payment is the operator's business; the result reaches the product as the operator's decisions in [Contracts](./contracts.md), and the frontend refreshes what the tenant may do when it next asks.
