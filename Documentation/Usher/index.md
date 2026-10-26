---
title: Usher
description: Usher is the Showrunner support part - issue reporting and support conversations for applications built on Cratis.
---

Usher is the support part of Showrunner. It lets the users of an application report issues and hold support conversations with the people running it, without the application building that machinery itself.

Usher is a library, not a service. The application hosts it:

- `Cratis.Showrunner.Usher` adds Usher's commands, queries and events to the application's Arc host and Chronicle event store.
- `Cratis.Showrunner.Usher.Contracts` holds the public events an application and a support desk exchange; see [Contracts](./contracts.md).
- `@cratis/showrunner.usher` is the generated TypeScript proxies for Usher's commands and queries.
- `@cratis/showrunner.usher.react` is the React components that drive them, built on `@cratis/components`.

Usher is in its earliest stage: the contracts are defined, and the other packages carry no behavior yet. This documentation grows with each slice that lands.
