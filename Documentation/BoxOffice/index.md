---
title: "Box Office: licensing a managed product"
description: Box Office lets a managed product enforce its operator's licensing decisions locally, from trial and seat access to modules and compute allowances.
---

Box Office is the licensing part of Showrunner: it lets a managed product enforce its operator's decisions about what each tenant may use. A tenant is a customer organization's space in the product; the operator decides its trial, seat capacity, module access and compute allowances.

Without a shared licensing contract, each product has to translate those decisions into its own access rules and reports. Box Office receives the operator's decisions through Chronicle and keeps them in the tenant's namespace, the tenant-specific area of the event store. Your application can check access there instead of calling the operator for each decision.

Box Office is a set of libraries, not a separate service. It runs inside the product's Arc and Chronicle application. The product still owns its tenant data and the work needed to create it.

## Connect your product to its operator

Start with [the product-side integration](./product.md). `Cratis.Showrunner.BoxOffice.Product` adds Box Office with `AddBoxOfficeProduct` and connects your tenant-creation code to the operator's registrations. It reports when a tenant is ready, provides seat and read-only checks, and reports who was active.

The two sides exchange public events from `Cratis.Showrunner.BoxOffice.Contracts`. The [contracts reference](./contracts.md) explains which side publishes each event and how it identifies the tenant and organization.

## Apply licensing decisions where work happens

`Cratis.Showrunner.BoxOffice` turns the received decisions into entitlements: the seats, modules and allowances a tenant may use. [Receiving and entitlements](./entitlements.md) explains how a decision reaches the tenant and becomes available to your application.

Use those entitlements to [control module access](./gating.md), [assign seats](./seats.md) and [admit compute work](./compute-admission.md). When a tenant is suspended or its trial closes without seats, [read-only enforcement](./suspension.md) refuses commands while leaving queries available. Work outside the command pipeline needs an explicit access check.

Your frontend can explain that access to the person using the product. `@cratis/showrunner.boxoffice` provides [generated TypeScript proxies](./frontend-proxies.md); `@cratis/showrunner.boxoffice.react` supplies [React components](./react.md) for entitlements, module filtering, trial and suspension banners, and seats, usage and plan pages. [Customer portal buttons](./portal.md) take the person to the operator to manage their subscription.

## Declare what the product sells

`Cratis.Showrunner.BoxOffice.Offering` lets a product [declare its modules, meters and seat licensing](./offering.md) and report them to the operator. It also [meters compute per tenant and day](./metering.md), so the operator receives usage alongside the product's declaration.

On the operator side, `Cratis.Showrunner.BoxOffice.Catalog` holds the offers for sale, their prices and pricing arithmetic, and publishes the catalog to products. See [Catalog](./catalog.md) when you need to define those offers rather than consume licensing decisions in a product.
