---
title: Usher
description: Add issue reporting and support conversations to Cratis applications with Showrunner Usher backend libraries, public event contracts and React components.
---

Usher is the support part of Showrunner. It lets the users of an application report issues and hold support conversations with the people running it, without the application building that machinery itself.

Usher is a library, not a service. The application hosts it:

- `Cratis.Showrunner.Usher` adds Usher's commands, queries and events to the application's Arc host and Chronicle event store; see [Backend](./backend.md).
- `Cratis.Showrunner.Usher.Contracts` holds the public events an application and a support desk exchange; see [Contracts](./contracts.md).
- `@cratis/showrunner.usher` is the generated TypeScript proxies for Usher's commands and queries, and helpers for reporting errors; see [Frontend proxies](./frontend-proxies.md).
- `@cratis/showrunner.usher.react` is the React components that drive them, built on `@cratis/components`; see [React components](./react.md).

Usher covers the whole path of an issue: reporting one, capturing and fingerprinting errors, holding the conversation, letting the reporter grant support time-limited access, and publishing the facts to the desk. The pages below follow that path; start with [Backend](./backend.md).
