---
title: Showrunner packages
description: Every Showrunner NuGet package, npm package, container image and Helm chart, what it contains and which part's documentation to read.
---

Showrunner ships backend packages on NuGet, TypeScript proxies and React components on npm, and Stage Manager as a container image and Helm chart. Choose the packages for the part you are hosting, then add its proxies or React components if the application has a frontend.

- A **backend** package goes in the Arc host that owns the part.
- A **contracts** package holds the public events two applications exchange. Both sides reference it, so neither depends on the other's internals.
- A **proxies** package (`@cratis/showrunner.<part>`) is generated TypeScript for the part's commands and queries, with no UI framework dependency.
- A **React** package (`@cratis/showrunner.<part>.react`) holds the components and hooks, built on `@cratis/arc.react` and `@cratis/components`.

All packages are released under one semantic version. Stage Manager's image and chart are released with that same version.

| Package | Registry | What it is | Documentation |
| --- | --- | --- | --- |
| `Cratis.Showrunner.BoxOffice` | NuGet | Product-side licensing: receives the operator's decisions and enforces tenant entitlements | [Box Office](./BoxOffice/index.md) |
| `Cratis.Showrunner.BoxOffice.Catalog` | NuGet | Box Office's catalog — offers (seat plans, modules, compute packs), prices and the pricing arithmetic, published to products | [Box Office](./BoxOffice/index.md) |
| `Cratis.Showrunner.BoxOffice.Offering` | NuGet | Box Office for a product — declares what the product sells (modules, meters, seat licensing) and measures its compute per tenant and day | [Box Office](./BoxOffice/index.md) |
| `Cratis.Showrunner.BoxOffice.Product` | NuGet | Box Office for a managed product, in one call — creates the tenants the operator registers and reports them ready, answers whether a person may take a seat and whether a tenant is read-only, and reports who was active | [Box Office](./BoxOffice/index.md) |
| `Cratis.Showrunner.BoxOffice.Contracts` | NuGet | Box Office's event contracts: what an operator decides about a tenant and what a product reports back | [Box Office](./BoxOffice/index.md) |
| `Cratis.Showrunner.BoxOffice.Product.Availability` | NuGet | Optional: lets a Choreography availability gate read what Box Office entitles the tenant to, as the host-provided `entitlements` context value | [Box Office](./BoxOffice/index.md) |
| `Cratis.Showrunner.Payments` | NuGet | The provider-neutral payment contract: `IPaymentProvider`, its requests and results, and `IPaymentWebhookHandler` | [Payments](./Payments/index.md) |
| `Cratis.Showrunner.Payments.Mollie` | NuGet | The Mollie implementation of the payment contract and its webhook endpoint | [Payments](./Payments/index.md) |
| `Cratis.Showrunner.Herald.Contracts` | NuGet | Herald's event contracts: the facts recorded when an email was sent, failed or bounced | [Herald](./Herald/index.md) |
| `Cratis.Showrunner.Herald` | NuGet | Herald's backend — templates, strict rendering, images, SMTP and folder transports, queued delivery with retries, recorded in the application's Chronicle event store | [Herald](./Herald/index.md) |
| `cratis/showrunner-operator-image`, `cratis/showrunner-operator` | Docker Hub, Helm (OCI) | Stage Manager, the Kubernetes operator: the container image and, in a separate repository, the Helm chart released with the same version | [Stage Manager](./StageManager/index.md) |
| `Cratis.Showrunner.Usher.Contracts` | NuGet | Usher's event contracts, for hosts that react to what Usher records | [Usher](./Usher/index.md) |
| `Cratis.Showrunner.Cue` | NuGet | Cue's backend — records which guided tours each person has started, built on Arc and Chronicle | [Cue](./Cue/index.md) |
| `Cratis.Showrunner.Marquee.Contracts` | NuGet | Marquee's event contracts: the public events a product receives announcements through | [Marquee](./Marquee/index.md) |
| `Cratis.Showrunner.GuestList` | NuGet | The product side of audiences: records an operator's grants and answers which audiences apply | [GuestList](./GuestList/index.md) |
| `Cratis.Showrunner.GuestList.Authoring` | NuGet | The operator side of audiences: commands, queries and the outbox for granting and withdrawing | [GuestList](./GuestList/index.md) |
| `Cratis.Showrunner.GuestList.Availability` | NuGet | Optional: feeds the Choreography availability engine from the audiences a product has recorded locally | [GuestList](./GuestList/index.md) |
| `Cratis.Showrunner.GuestList.Contracts` | NuGet | GuestList's event contracts: the public events audiences are granted and withdrawn through | [GuestList](./GuestList/index.md) |
| `Cratis.Showrunner.Marquee.Authoring` | NuGet | Marquee's authoring backend — commands, query and outbox for writing announcements, built on Arc and Chronicle | [Marquee](./Marquee/index.md) |
| `Cratis.Showrunner.Marquee` | NuGet | Marquee's backend for products — receives announcements once, shows each tenant what its audience matches and records acknowledgements | [Marquee](./Marquee/index.md) |
| `Cratis.Showrunner.Usher` | NuGet | Usher's backend — Arc commands and queries over Chronicle | [Usher](./Usher/index.md) |
| `Cratis.Showrunner.Notes.Contracts` | NuGet | Notes' event contracts: the public events an application and the people building it exchange about product feedback | [Notes](./Notes/index.md) |
| `Cratis.Showrunner.Notes` | NuGet | Notes' backend for applications — commands to give, suggest and withdraw notes, the live list of a user's notes with their status, and the outbox that publishes them | [Notes](./Notes/index.md) |
| `Cratis.Showrunner.Notes.Collecting` | NuGet | Notes' receiving side for the builder of an application — reads the applications' notes into the tenant's namespace and answers them | [Notes](./Notes/index.md) |
| `@cratis/showrunner.boxoffice` | npm | Generated TypeScript proxies for the product's licensing frontend | [Box Office](./BoxOffice/index.md) |
| `@cratis/showrunner.boxoffice.react` | npm | React components for entitlements, module filtering, trials, suspension, seats, usage and plans | [Box Office](./BoxOffice/index.md) |
| `@cratis/showrunner.cue` | npm | Generated TypeScript proxies for Cue's command and observable query | [Cue](./Cue/index.md) |
| `@cratis/showrunner.cue.react` | npm | The Cue guided-tour player for React: the script types, the overlay, the pointer, the voice-over contract and the hooks that connect a tour to the backend, built on `@cratis/arc.react` and `@cratis/components` | [Cue](./Cue/index.md) |
| `@cratis/showrunner.marquee` | npm | Generated TypeScript proxies for Marquee's receiving commands and queries | [Marquee](./Marquee/index.md) |
| `@cratis/showrunner.marquee.authoring` | npm | Generated TypeScript proxies for Marquee's authoring commands and query | [Marquee](./Marquee/index.md) |
| `@cratis/showrunner.marquee.react` | npm | The Marquee drop-in React component: banners and dialogs for a product's announcements, built on `@cratis/arc.react` and `@cratis/components` | [Marquee](./Marquee/index.md) |
| `@cratis/showrunner.usher` | npm | Generated TypeScript proxies for Usher's commands and queries | [Usher](./Usher/index.md) |
| `@cratis/showrunner.usher.react` | npm | React components for Usher, built on `@cratis/arc.react` and `@cratis/components` | [Usher](./Usher/index.md) |
| `@cratis/showrunner.notes` | npm | Generated TypeScript proxies for Notes' commands and queries | [Notes](./Notes/index.md) |
| `@cratis/showrunner.notes.react` | npm | React components for Notes: the feedback button with its context drop-down, the feedback dialogs, the component picker and the user's own feedback, built on `@cratis/arc.react` and `@cratis/components` | [Notes](./Notes/index.md) |
