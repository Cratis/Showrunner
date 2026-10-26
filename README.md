# Showrunner

> Showrunner is a set of ready-made product parts for applications built on [Cratis](https://www.cratis.io) — drop-in backend and frontend packages, built as Arc and Chronicle vertical slices.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Build](https://github.com/Cratis/Showrunner/actions/workflows/build.yml/badge.svg)](https://github.com/Cratis/Showrunner/actions/workflows/build.yml)
[![Publish](https://github.com/Cratis/Showrunner/actions/workflows/publish.yml/badge.svg)](https://github.com/Cratis/Showrunner/actions/workflows/publish.yml)
[![Discord](https://img.shields.io/discord/1182595891576717413?label=Discord&logo=discord&logoColor=white)](https://discord.gg/kt4AMpV8WV)

## Usher

**Usher** is the support part: it lets the users of an application report issues and hold support conversations with the people running it. It is a library an application hosts, not a service of its own — the backend package adds Usher's commands, queries and events to the host's Arc application and Chronicle event store, and the frontend packages give the host's React UI the generated proxies and components to drive them.

Usher is in its earliest stage: the packages are scaffolded and published, but carry no behavior yet. See [Documentation](./Documentation/index.md).

## Packages

| Package | Registry | What it is |
| --- | --- | --- |
| `Cratis.Showrunner.Usher.Contracts` | NuGet | Usher's event contracts, for hosts that react to what Usher records |
| `Cratis.Showrunner.Usher` | NuGet | Usher's backend — Arc commands and queries over Chronicle |
| `@cratis/showrunner.usher` | npm | The generated TypeScript proxies for Usher's commands and queries; no UI framework dependency |
| `@cratis/showrunner.usher.react` | npm | React components for Usher, built on `@cratis/arc.react` and `@cratis/components` |

## Building

Prerequisites: the .NET SDK from [`global.json`](./global.json) (10.0.400 or later) and Node.js 20 or later with Corepack enabled (`corepack enable`); Yarn itself is checked in under `.yarn/releases`.

```shell
# Backend: Debug builds compile the inline specs and regenerate the TypeScript proxies
dotnet build Showrunner.slnx --configuration Debug
dotnet test Showrunner.slnx --configuration Debug --no-build

# Release is warnings-as-errors; clearing CratisProxiesOutputPath skips proxy generation
dotnet build Showrunner.slnx --configuration Release -p:CratisProxiesOutputPath=

# Frontend
yarn install
yarn lint:ci
yarn compile
yarn test
yarn build
```

Specifications live beside the code they specify — `for_*/when_*/` folders compiled into Debug builds only for .NET, and Vitest specs in the same layout for TypeScript.

The end-to-end specifications in [`Integration`](./Integration) run Usher and a desk against a real Chronicle kernel in a container, so `dotnet test Showrunner.slnx` needs Docker. Set `USHER_CHRONICLE_IMAGE` to run them against another kernel image, and `USHER_INTEGRATION_LOG` to a file path to keep the application's log.

## Releasing

Every pull request carries exactly one of the `major`, `minor`, `patch` or `no-release` labels. Merging a version-labelled pull request to `main` releases all four packages under one semantic version through [`cratis/release-action`](https://github.com/Cratis/release-action) — NuGet packages through NuGet trusted publishing and npm packages through npm trusted publishing with provenance.

## Part of the Cratis ecosystem

Showrunner is a [Cratis](https://www.cratis.io) project, built on event sourcing with [Chronicle](https://github.com/Cratis/Chronicle) and CQRS with [Arc](https://github.com/Cratis/Arc), with frontends on [`@cratis/components`](https://github.com/Cratis/Components). Join the community on [Discord](https://discord.gg/kt4AMpV8WV).

## License

Showrunner is licensed under the [MIT License](LICENSE) and free to use.
