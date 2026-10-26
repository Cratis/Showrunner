---
title: Frontend proxies
description: The @cratis/showrunner.usher package - the generated TypeScript proxies for Usher's commands and queries, their slice location, and the helpers that turn an error into what a report sends.
---

`@cratis/showrunner.usher` is the TypeScript side of `Cratis.Showrunner.Usher`: the proxies Arc generates for Usher's commands and queries, and a few helpers for reporting errors. It has no components; `@cratis/showrunner.usher.react` builds the dialogs on top of it.

## Install

```shell
yarn add @cratis/showrunner.usher @cratis/arc @cratis/arc.react @cratis/fundamentals
```

`@cratis/arc`, `@cratis/arc.react` and `@cratis/fundamentals` are peer dependencies, so the application decides their versions. The proxies need `@cratis/arc.react` because Arc generates a React hook, such as `ReportError.use()`, on every command and query proxy. Use the package version that matches the `Cratis.Showrunner.Usher` NuGet package: both are released together with the same version.

## Commands and queries

Every command and query of the [backend](./backend.md) has a proxy, exported from the package root:

```ts
import { ReportIssue, MyIssues, AccessDuration } from '@cratis/showrunner.usher';
import { Guid } from '@cratis/fundamentals';

const report = new ReportIssue();
report.issueId = Guid.create();
report.summary = 'The invoice is not sent';
report.description = 'I pressed send and nothing happened.';
report.accessDuration = AccessDuration.oneHour;
const result = await report.execute();
```

The proxies are generated from the C# code; never edit them. They live in the folder of their slice, so `Reporting/ReportError/ReportError.ts` is the proxy for the `ReportError` command in the `Reporting` feature.

## Slice location

Each proxy carries the slice it belongs to as a static `sliceLocation`, the same location the backend resolves for the command or query:

```ts
import { SliceLocation } from '@cratis/arc/reflection';
import { ReportError } from '@cratis/showrunner.usher';

ReportError.sliceLocation.toString();     // 'Reporting/ReportError'
SliceLocation.of(new ReportError());      // the same location, from an instance
```

Usher's slices sit directly under the `Cratis.Showrunner.Usher` namespace, so their module is empty: `ReportError` is feature `Reporting`, slice `ReportError`.

## Turn an error into what a report sends

The `ReportError` command wants the error message and type, the correlation ids, the stack frames and where it happened. These helpers build that as `ErrorDetails` from whatever the application has at hand, and never return null or undefined parts: what is not known is empty.

| Function | From |
| --- | --- |
| `errorDetailsFromError(error, location?)` | Anything thrown: a JavaScript `Error`, or any other value. |
| `errorDetailsFromCommandResult(result, command?, location?)` | The result of a failed Arc command. |
| `errorDetailsFromQueryResult(result, query?, location?, correlationId?)` | The result of a failed Arc query. |
| `parseStackTrace(stackTrace?)` | A stack trace: a JavaScript stack from Chromium, Node.js, Firefox or Safari, or the .NET stack trace Arc returns. |

```ts
import { errorDetailsFromCommandResult, ReportError } from '@cratis/showrunner.usher';

const result = await payInvoice.execute();
if (!result.isSuccess) {
    const details = errorDetailsFromCommandResult(result, payInvoice, location);

    const report = new ReportError();
    report.location = details.location;
    report.errorMessage = details.message;
    report.errorType = details.type;
    report.correlationIds = details.correlationIds;
    report.stackFrames = details.stackFrames;
}
```

- **Where it happened.** When the failing command or query is passed and its proxy declares a slice location, that is used. Otherwise the `location` passed is used, typically the one `useSliceContext()` from `@cratis/arc.react/slices` gives the component that shows the error. Without either, it is empty.
- **The type.** A JavaScript error's type is its `name`, or its class when a subclass keeps the name `Error`. Arc does not return the .NET type of a failing command or query, so for those the type is empty.
- **Correlation ids.** A command result carries its correlation id, and it is included. A query result does not; pass it when you know it.

For a failing command, the client's view is only a fallback. The backend captures the error of the failing command itself, and when the report carries the command's correlation id, Usher records the error as the backend saw it, type and stack frames included; see [error capture](./backend.md#error-capture).

## How the proxies are built

The Debug build of `Cratis.Showrunner.Usher` generates the proxies into `Source/JavaScript/Usher`, and they are committed. The npm build compiles the committed proxies with the helpers; it does not run the .NET build. Continuous integration rebuilds Debug and fails when the committed proxies differ from what the backend generates, so a backend change is never released with stale proxies.

```shell
dotnet build Showrunner.slnx -c Debug   # regenerates Source/JavaScript/Usher
yarn build                              # builds the npm packages from the committed proxies
```

The package contains the compiled JavaScript, the type declarations and the TypeScript sources; specifications are left out.
