---
title: Marquee
description: Marquee delivers announcements from the people running a product to the people using it.
---

Marquee is the announcements part of Showrunner. The people running a product publish an announcement, such as a maintenance window or a new module, and the product shows it to the audience it names.

Marquee is a set of libraries, not a service. Its first package is the [contracts](./contracts.md): the public events a product receives announcements through. Authoring, receiving and the React components arrive in later packages.

Marquee does not depend on any other Showrunner part. Who an announcement reaches by module or by plan is answered by an `IAudienceEntitlements` the host plugs in, so a product that sells nothing still gets announcements.
