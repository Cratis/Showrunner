---
title: Showrunner
description: Equip Cratis applications with Showrunner packages for support, licensing, announcements, feedback and email, plus a Kubernetes deployment operator.
---

Showrunner is a set of ready-made product parts for applications built on [Cratis](https://www.cratis.io): support, licensing, announcements, feedback and email, plus the Kubernetes operator that runs an application.

Without Showrunner, every product that needs a way to report problems, license a tenant, announce a maintenance window, collect feedback or send an invitation builds that machinery again. With Showrunner you add a package, call one registration method and drop in the component: the part brings its own commands, queries, events and screens, and records everything as facts in your application's own Chronicle event store.

## How the parts fit

Each part is a set of packages built as Arc and Chronicle vertical slices:

- **A backend package** you add to your Arc host. It registers its commands, queries, reactors and events; there is nothing else to wire.
- **A contracts package** with the public events the part exchanges with another application, such as the operator of a managed product or the people building your application. Both sides reference it, so neither depends on the other's internals.
- **Generated TypeScript proxies** for the part's commands and queries, with no UI framework dependency.
- **React components** built on `@cratis/arc.react` and `@cratis/components`, for the parts that have a user interface.

The library parts record what they decide as events in your application's own event store, so you can read it with a projection, react to it with a reactor and inspect it with the Chronicle tools you already use. Stage Manager is the exception: it is a service you install in a cluster, not a library you host.

## Start here

1. Pick the part that removes the work you would otherwise build from the table below.
2. Open its overview, which lists the packages and links to the page that adds it to an application.
3. Follow that page to add the part to your application.

Where a part exchanges public events with another application, its contracts page names the events and who publishes them.

## Parts

| Part | What it does |
| --- | --- |
| [Usher](./Usher/index.md) | Support: issue reporting and support conversations between an application's users and the people running it |
| [Box Office](./BoxOffice/index.md) | Licensing: trials, seats, modules, compute allowances and suspension of a managed product's tenants |
| [Marquee](./Marquee/index.md) | Announcements: what the people running a product tell the people using it, with an audience, a display and an exact active window |
| [Notes](./Notes/index.md) | Feedback: product feedback from an application's users, tied to the slice, screen or component they were looking at, delivered as events to the people building the application |
| [Herald](./Herald/index.md) | Email: templated, email-safe messages with images, sent over SMTP, retried when the server fails and recorded as facts |
| [Stage Manager](./StageManager/index.md) | The Kubernetes operator: runs an application, or a published Stage application, with its AuthProxy and its own Ante from a Cratis environment, behind one public entry point |

## When Showrunner is the wrong fit

A part is opinionated: it records its own events and shows its own screens. If your product needs a support, licensing or feedback model that differs from the part's, build your own slice on Arc and Chronicle instead of bending the part. Showrunner parts use Cratis conventions throughout, so they do not suit an application that does not use Arc and Chronicle.
