# Showrunner

Cratis Showrunner adds support, licensing, announcements, product feedback and email to applications built on [Cratis](https://www.cratis.io). It provides .NET libraries, TypeScript clients generated from backend commands and queries, and React components. It also includes Stage Manager, a Kubernetes operator for running applications.

Choose the part that matches what your application needs. The libraries run inside the application that hosts them; Stage Manager runs as a service in your cluster. The sections below lead to setup instructions for each part.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Build](https://github.com/Cratis/Showrunner/actions/workflows/build.yml/badge.svg)](https://github.com/Cratis/Showrunner/actions/workflows/build.yml)
[![Publish](https://github.com/Cratis/Showrunner/actions/workflows/publish.yml/badge.svg)](https://github.com/Cratis/Showrunner/actions/workflows/publish.yml)
[![Discord](https://img.shields.io/discord/1182595891576717413?label=Discord&logo=discord&logoColor=white)](https://discord.gg/kt4AMpV8WV)

## Usher

Usher lets users report issues and carry on support conversations from the application. Its backend adds commands, queries and events to the host's Arc application and Chronicle event store; its frontend packages supply the generated TypeScript clients and React components.

Usher captures errors and lets a reporter grant time-limited support access through AuthProxy's on-behalf sessions, where support acts as the reporter. Public events connect the application to a support desk. Start with [the Usher documentation](./Documentation/Usher/index.md) to connect those two sides.

## Box Office

Box Office lets a managed product honor the licensing decisions its operator makes for each tenant: trials, seats, modules, compute allowances and suspension. The product receives those decisions and reports back through public events. The operator's catalog holds offers and prices; the product declares what it sells and meters compute use. Follow [the Box Office documentation](./Documentation/BoxOffice/index.md) to separate the operator's responsibilities from the product's.

## Marquee

Marquee brings product announcements to the people they concern. The people running the product choose the audience, display and time window; the product shows the announcement as a banner or dialog. Add `<Marquee />` to the application shell to use its queries and commands. Follow [the Marquee documentation](./Documentation/Marquee/index.md) to add announcements or host the authoring side.

## Notes

Notes connects product feedback to what the user was looking at: a slice of application behavior, a screen or a single component. The people building the application receive that context as events and answer the note. The application hosts `Cratis.Showrunner.Notes`; the builder hosts `Cratis.Showrunner.Notes.Collecting`. Use [the Notes documentation](./Documentation/Notes/index.md) to connect feedback collection and replies.

## Herald

Herald sends application email from templates and records what happened. The application supplies an email kind, such as an invitation, a language and its values. Herald fills the template, makes the HTML safe for email clients, embeds images and sends it over SMTP. Failed sends are queued and retried. Start with [the Herald documentation](./Documentation/Herald/index.md) to configure templates and delivery.

## Stage Manager

Stage Manager runs applications in Kubernetes. Install it as a service with the `cratis/showrunner-operator` Helm chart. A `Performance` runs an application with its AuthProxy and, when onboarding is configured, its own Ante from a Cratis environment. The application is reachable only through AuthProxy. A Performance can also run a published Stage application.

`Performance` is reconciled: the operator acts on it to run the application. The other resources have schemas only; nothing acts on them yet. Follow [the Stage Manager documentation](./Documentation/StageManager/index.md) to install the operator and run an application.

## Packages

Choose the backend packages for the part you are hosting, then add its TypeScript proxies or React components if the application needs a frontend. The contract packages contain the public events exchanged between hosts. Stage Manager ships separately as a container image and Helm chart, not a NuGet package.

| Package | Registry | What it is |
| --- | --- | --- |
| `Cratis.Showrunner.BoxOffice` | NuGet | Product-side licensing: receives the operator's decisions and enforces tenant entitlements |
| `Cratis.Showrunner.BoxOffice.Catalog` | NuGet | Box Office's catalog — offers (seat plans, modules, compute packs), prices and the pricing arithmetic, published to products |
| `Cratis.Showrunner.BoxOffice.Offering` | NuGet | Box Office for a product — declares what the product sells (modules, meters, seat licensing) and measures its compute per tenant and day |
| `Cratis.Showrunner.BoxOffice.Product` | NuGet | Box Office for a managed product, in one call — creates the tenants the operator registers and reports them ready, answers whether a person may take a seat and whether a tenant is read-only, and reports who was active |
| `Cratis.Showrunner.BoxOffice.Contracts` | NuGet | Box Office's event contracts: what an operator decides about a tenant and what a product reports back |
| `Cratis.Showrunner.Herald.Contracts` | NuGet | Herald's event contracts: the facts recorded when an email was sent, failed or bounced |
| `Cratis.Showrunner.Herald` | NuGet | Herald's backend — templates, strict rendering, images, SMTP and folder transports, queued delivery with retries, recorded in the application's Chronicle event store |
| `cratis/showrunner-operator-image`, `cratis/showrunner-operator` | Docker Hub, Helm (OCI) | Stage Manager, the Kubernetes operator: the container image and, in a separate repository, the Helm chart released with the same version |
| `Cratis.Showrunner.Usher.Contracts` | NuGet | Usher's event contracts, for hosts that react to what Usher records |
| `Cratis.Showrunner.Marquee.Contracts` | NuGet | Marquee's event contracts: the public events a product receives announcements through |
| `Cratis.Showrunner.Marquee.Authoring` | NuGet | Marquee's authoring backend — commands, query and outbox for writing announcements, built on Arc and Chronicle |
| `Cratis.Showrunner.Marquee` | NuGet | Marquee's backend for products — receives announcements once, shows each tenant what its audience matches and records acknowledgements |
| `Cratis.Showrunner.Usher` | NuGet | Usher's backend — Arc commands and queries over Chronicle |
| `Cratis.Showrunner.Notes.Contracts` | NuGet | Notes' event contracts: the public events an application and the people building it exchange about product feedback |
| `Cratis.Showrunner.Notes` | NuGet | Notes' backend for applications — commands to give, suggest and withdraw notes, the live list of a user's notes with their status, and the outbox that publishes them |
| `Cratis.Showrunner.Notes.Collecting` | NuGet | Notes' receiving side for the builder of an application — reads the applications' notes into the tenant's namespace and answers them |
| `@cratis/showrunner.boxoffice` | npm | Generated TypeScript proxies for the product's licensing frontend |
| `@cratis/showrunner.boxoffice.react` | npm | React components for entitlements, module filtering, trials, suspension, seats, usage and plans |
| `@cratis/showrunner.marquee` | npm | Generated TypeScript proxies for Marquee's receiving commands and queries |
| `@cratis/showrunner.marquee.authoring` | npm | Generated TypeScript proxies for Marquee's authoring commands and query |
| `@cratis/showrunner.marquee.react` | npm | The Marquee drop-in React component: banners and dialogs for a product's announcements, built on `@cratis/arc.react` and `@cratis/components` |
| `@cratis/showrunner.usher` | npm | Generated TypeScript proxies for Usher's commands and queries |
| `@cratis/showrunner.usher.react` | npm | React components for Usher, built on `@cratis/arc.react` and `@cratis/components` |
| `@cratis/showrunner.notes` | npm | Generated TypeScript proxies for Notes' commands and queries |
| `@cratis/showrunner.notes.react` | npm | React components for Notes: the feedback button with its context drop-down, the feedback dialogs, the component picker and the user's own feedback, built on `@cratis/arc.react` and `@cratis/components` |

## Building

To work on Showrunner, use the .NET 11 SDK selected by [`global.json`](./global.json), the .NET 10 SDK for the libraries' net10.0 targets and inline specifications, and Node.js 20 or later with Corepack enabled (`corepack enable`). Yarn is checked in under `.yarn/releases`.

Run the commands below from the repository root. The full solution test command includes container-based integration specifications, so Docker must be running.

```shell
# Backend: Debug builds compile the inline specs and regenerate the TypeScript proxies
dotnet build Showrunner.slnx --configuration Debug
dotnet test Showrunner.slnx --configuration Debug --no-build

# Release is warnings-as-errors; clearing CratisProxiesOutputPath skips proxy generation
dotnet build Showrunner.slnx --configuration Release -p:CratisProxiesOutputPath=

# Frontend
yarn install
yarn clean
yarn lint:ci
yarn compile
yarn build
yarn test
```

Run `yarn clean` before `yarn compile`: a stale `tsc -b` build-info cache can report success and hide real type errors.

Specifications live beside the code they specify: `for_*/when_*/` folders compiled into Debug builds only for .NET, and Vitest specs in the same layout for TypeScript. The Debug build regenerates TypeScript proxies from the backend; do not edit the generated proxies by hand. The pull request build checks that regeneration leaves the committed proxies unchanged.

For a focused integration run, [`Integration`](./Integration) runs Usher and a desk against Chronicle in a container. [`Integration.Notes`](./Integration.Notes) runs an application and a collecting host using the same container fixture code. For these two projects, set `USHER_CHRONICLE_IMAGE` to choose another kernel image and `USHER_INTEGRATION_LOG` to a file path to keep the hosts' logs.

`Integration.Herald/` also tests against Chronicle in a container; its image override is `HERALD_CHRONICLE_IMAGE`. `Integration.BoxOffice/` tests metering storage against MongoDB in Docker.

## Releasing

Every pull request carries exactly one of the `major`, `minor`, `patch` or `no-release` labels. Merging a version-labelled pull request to `main` releases the NuGet and npm packages under one semantic version through [`cratis/release-action`](https://github.com/Cratis/release-action). Use `no-release` to skip publishing. Stage Manager's container image and Helm chart are released with the same version.

NuGet and npm use trusted publishing through OpenID Connect (OIDC), with provenance for npm. Build and pack jobs have no publishing rights; separate publish jobs receive short-lived publishing credentials and push the packed artifacts. The npm publisher disables lifecycle scripts when publishing the tarballs.

## Part of the Cratis ecosystem

Showrunner is a [Cratis](https://www.cratis.io) project. Its application libraries use [Chronicle](https://github.com/Cratis/Chronicle) to store changes as events. Those with commands and queries use [Arc](https://github.com/Cratis/Arc) to handle writes and reads separately, the pattern called CQRS. The React components build on [`@cratis/components`](https://github.com/Cratis/Components). Join the community on [Discord](https://discord.gg/kt4AMpV8WV).

## License

See the [MIT License](LICENSE) for the terms covering this repository.
