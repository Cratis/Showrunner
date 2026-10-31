---
title: Marquee
description: Marquee delivers announcements from the people running a product to the people using it.
---

Marquee is the announcements part of Showrunner. The people running a product publish an announcement, such as a maintenance window or a new module, and the product shows it to the audience it names.

Marquee is a set of libraries, not a service. Each runs in the Arc and Chronicle process of the application that hosts it, the way [Usher](../Usher/index.md) does:

- [Contracts](./contracts.md): the public events a product receives announcements through.
- [Authoring](./authoring.md): the commands and the query for writing announcements, and the outbox that publishes them.
- [Receiving](./receiving.md): how a product records announcements once for all its tenants and decides which ones a tenant sees.
- [Acknowledging](./acknowledging.md): how a person dismisses a dialog once, on every device.
- [Frontend proxies](./frontend-proxies.md): the generated TypeScript proxies.
- [Event reference](./event-reference.md): every event, the contract generations and what to do when one changes.

The React component, `@cratis/showrunner.marquee.react`, shows the announcements the way the people using a product expect: banners at the top and dialogs one at a time. Start with [how Marquee works](./how-marquee-works.md), then [add announcements to a product](./add-announcements-to-a-product.md).

## Guides

- [How Marquee works](./how-marquee-works.md): authoring, the contract, receiving, audiences and the component.
- [Add announcements to a product](./add-announcements-to-a-product.md): the backend package and `<Marquee />`.
- [Author announcements without Direct](./author-without-direct.md): host the authoring backend in your own application.
- [Replace the banner or the dialog](./replace-the-banner-or-the-dialog.md): slots and `useAnnouncements`.

Marquee does not depend on any other Showrunner part. Who an announcement reaches by module or by plan is answered by an `IAudienceEntitlements` the host plugs in, so a product that sells nothing still gets announcements.
