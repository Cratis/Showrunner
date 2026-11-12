---
title: GuestList
description: Audience grants for a product - which audiences a tenant, a user or the system is in - published by an operator and received once by the product.
---

GuestList carries one decision from the people running a product to the product itself: this tenant, this user or the system is in this audience. The audience is a name the model declares, such as `Plus`. What the product does with it, for example gating a feature on it, is the product's own business; GuestList only records who is on which list.

Nothing in GuestList depends on Direct. Box Office module entitlements are separate and unchanged: Box Office still decides what a tenant has bought, and GuestList decides which audiences someone is in.

- [How GuestList works](./how-guestlist-works.md): the three levels, how the newest decision wins and what stays when the link is down.
- [Receive audiences in a product](./receive-audiences-in-a-product.md): add the product side and read what it recorded.
- [Contracts](./contracts.md): the public events, their fields and the schema snapshot.

| Package | What it is |
| --- | --- |
| `Cratis.Showrunner.GuestList.Contracts` | The public events and the types they carry |
| `Cratis.Showrunner.GuestList` | The product side: inbox, read models and queries |
