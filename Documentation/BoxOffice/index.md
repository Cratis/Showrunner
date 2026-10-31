---
title: Box Office
description: Box Office is the Showrunner part for licensing a managed product - trials, seats, modules, compute allowances and suspension - decided by the operator and honored by the product.
---

Box Office is the licensing part of Showrunner. The operator of a managed product, such as Direct, decides what each tenant may do: how long its trial runs, how many seats it has, which modules it has and how much compute it may use. The product honors those decisions and reports back what happened in it.

Box Office is a library set, not a service. The operator and the product exchange public events through Chronicle:

- `Cratis.Showrunner.BoxOffice.Contracts` holds the public events both sides exchange; see [Contracts](./contracts.md).
- `Cratis.Showrunner.BoxOffice` runs inside the product's own Arc and Chronicle process and honors the operator's decisions: it receives them, answers what a tenant may use, refuses commands from a tenant that is suspended and licenses seats; see [Receiving and entitlements](./entitlements.md), [Suspension and read-only tenants](./suspension.md) and [Seat licensing](./seats.md).
- `@cratis/showrunner.boxoffice` is the generated TypeScript proxies for the product's frontend; see [Frontend proxies](./frontend-proxies.md).
- `Cratis.Showrunner.BoxOffice.Catalog` holds the offers the operator sells, their prices and the pricing arithmetic, and publishes the catalog to products; see [Catalog](./catalog.md).
- `Cratis.Showrunner.BoxOffice.Offering` is what a product references: it declares what the product sells and reports it to the operator, see [Offering](./offering.md), and it measures compute per tenant and day, see [Metering compute](./metering.md).

This documentation grows with each part of Box Office that lands.
