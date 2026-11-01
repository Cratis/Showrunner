---
title: Showrunner
description: Showrunner is a set of ready-made product parts for Cratis applications, delivered as NuGet and npm packages.
---

Showrunner is a set of ready-made product parts for applications built on [Cratis](https://www.cratis.io). Each part is a library: an application adds the backend package to its Arc host and Chronicle event store, and the frontend packages to its React UI.

## Parts

| Part | What it does |
| --- | --- |
| [Usher](./Usher/index.md) | Support: issue reporting and support conversations between an application's users and the people running it |
| [Box Office](./BoxOffice/index.md) | Licensing: trials, seats, modules, compute allowances and suspension of a managed product's tenants |
| [Marquee](./Marquee/index.md) | Announcements: what the people running a product tell the people using it, with an audience, a display and an exact active window |
| [Notes](./Notes/index.md) | Feedback: product feedback from an application's users, tied to the slice, screen or component they were looking at, delivered as events to the people building the application |
| [Herald](./Herald/index.md) | Email: templated, email-safe messages with images, sent over SMTP, retried when the server fails and recorded as facts |
| [Stage Manager](./StageManager/index.md) | The Kubernetes operator: runs an application with its AuthProxy and its own Ante from a Cratis environment, behind one public entry point |
