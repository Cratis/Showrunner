---
title: Box Office
description: Box Office is the Showrunner part for licensing a managed product - trials, seats, modules, compute allowances and suspension - decided by the operator and honored by the product.
---

Box Office is the licensing part of Showrunner. The operator of a managed product, such as Direct, decides what each tenant may do: how long its trial runs, how many seats it has, which modules it has and how much compute it may use. The product honors those decisions and reports back what happened in it.

Box Office is a library set, not a service. The operator and the product exchange public events through Chronicle:

- `Cratis.Showrunner.BoxOffice.Contracts` holds the public events both sides exchange; see [Contracts](./contracts.md).
- `Cratis.Showrunner.BoxOffice.Catalog` holds the offers the operator sells, their prices and the pricing arithmetic, and publishes the catalog to products; see [Catalog](./catalog.md).
- `Cratis.Showrunner.BoxOffice.Offering` is what a product references: it declares what the product sells and reports it to the operator, see [Offering](./offering.md), and it measures compute per tenant and day, see [Metering compute](./metering.md).

This documentation grows with each part of Box Office that lands.
