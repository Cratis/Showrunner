---
title: Add release notes to an application
description: A tutorial that adds the Playbill backend and React packages to an application, embeds its release notes at build time, serves them and shows a releases page, a what's new dialog and an unread badge.
---
In this tutorial you add Playbill to an application called `ledger`: the backend package that embeds and serves its release history, and the React components that show it. The application must already be an Arc application with a Chronicle event store and a React frontend that uses Cratis Components.

## 1. Add the backend

Reference `Cratis.Showrunner.Playbill`, call `AddPlaybill` next to `AddCratis`, and map the endpoints:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddPlaybill();

var app = builder.Build();
app.UseCratis();
app.MapPlaybill();
app.Run();
```

`AddPlaybill` reads the release history from the application's entry assembly, which is the project that holds the `ReleaseNotes` folder, and takes the running version from that assembly's informational version. Both can be changed:

```csharp
builder.AddPlaybill(options =>
{
    options.CurrentVersion = "2.3.0";
    options.AllowAnonymous = true;
});
```

`CurrentVersion` is the version shown as current. `AllowAnonymous` lets callers who are not signed in read the endpoints; without it they require a signed-in user. `Assemblies` names other assemblies to read the history from, for an application whose notes are not in its entry project.

## 2. Add the release notes

Create a `ReleaseNotes` folder in the project, with one folder for each version:

```text
Source/Host/ReleaseNotes/
  1.0.0/1.0.0.md
  1.1.0/1.1.0.md
```

A note is markdown whose sections use the headings of the release-note contract: `## Added`, `## Changed`, `## Fixed`, `## Removed`, `## Security` and `## Deprecated`. A paragraph before the first heading is shown as the lead of the release. Normally you do not write these by hand: [the capture action](./capture-release-notes.md) writes them when you ship.

Build the project. The folder is embedded automatically. Run the application and read `/.cratis/playbill/v1/releases` while signed in to see the list.

### Choose what is embedded

The targets read `ReleaseNotes` in the project directory. Two MSBuild properties change that, in the project file or on the command line:

| Property | Default | Meaning |
| --- | --- | --- |
| `PlaybillEmbedReleaseNotes` | `true` | Set to `false` to embed nothing. |
| `PlaybillReleaseNotesDirectory` | `$(MSBuildProjectDirectory)/ReleaseNotes` | The folder that holds one sub-folder per version. |

`versions.txt` and a `README.md` in the folder are not release notes and are left out of the assembly.

## 3. Install the packages

```shell
yarn add @cratis/showrunner.playbill.react @cratis/showrunner.playbill @cratis/arc @cratis/arc.react @cratis/components @cratis/fundamentals
```

`@cratis/arc`, `@cratis/arc.react`, `@cratis/components` and `@cratis/fundamentals` are peer dependencies, so the application decides their versions. Use the package version that matches the NuGet package; everything in Showrunner is released together with one version.

## 4. Import the stylesheet

Import it once, after the Cratis Components stylesheets:

```css
@import '@cratis/components/tokens';
@import '@cratis/components/styles';
@import '@cratis/components/theme';
@import '@cratis/showrunner.playbill.react/styles';
```

## 5. Show it

Put the dialog and the badge in the application shell, inside `<Arc>`. The dialog opens by itself when something is unread; the badge is the way back to the history:

```tsx
import { Arc } from '@cratis/arc.react';
import { CratisComponentsProvider } from '@cratis/components';
import { PlaybillBadge, PlaybillDialog } from '@cratis/showrunner.playbill.react';
import { ReactNode } from 'react';

export const AppShell = ({ children }: { children: ReactNode }) => (
    <CratisComponentsProvider>
        <Arc>
            <>
                <PlaybillDialog />
                <header>
                    <PlaybillBadge onClick={() => window.location.assign('/release-notes')} />
                </header>
                {children}
            </>
        </Arc>
    </CratisComponentsProvider>
);
```

Give the release notes a page of their own, and send the badge to it:

```tsx
import { Playbill } from '@cratis/showrunner.playbill.react';

export const ReleaseNotesPage = () => (
    <Playbill />
);
```

## 6. See it work

- A person who has seen nothing is shown the newest release in the dialog. Closing it records that they saw it, on every device.
- The badge shows how many releases are unread and clears when they are seen, on every open device.
- The page lists the releases newest first. Showing a release that has not been seen records it as seen.
- Ship another release: the badge counts it, and the dialog opens again with just that release.

The components read the endpoints of the page's own origin. [Customize the release-notes experience](./customize-the-release-notes-experience.md#read-from-another-endpoint) shows how to read another one.
