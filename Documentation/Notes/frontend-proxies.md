---
title: Frontend proxies
description: Call Showrunner Notes commands and queries with generated @cratis/showrunner.notes TypeScript proxies, without taking a dependency on a UI framework.
---

Arc generates a TypeScript proxy for every command and query. Notes ships them in `@cratis/showrunner.notes`, with no UI framework dependency.

| Proxy | Kind | Backend |
| --- | --- | --- |
| `GiveNoteOnContext`, `GiveNoteOnSlices`, `GiveNoteOnScreen`, `GiveNoteOnComponent` | Commands | `Giving/…` |
| `SuggestComponentPropertyChange` | Command | `Giving/SuggestComponentPropertyChange` |
| `WithdrawNote` | Command | `Withdrawing/WithdrawNote` |
| `MyNotes` (returns `MyNote[]`) | Observable query | `Listing/MyNote` |
| `Availability` (returns `NotesAvailability`) | Query | `Availability/NotesAvailability` |
| `MyNote`, `NotesAvailability`, `NoteContext`, `NoteScreen`, `NoteDialog`, `PickedComponent`, `SliceLocation`, `NoteKind`, `NoteStatus`, `NoteTarget`, `SuggestedPropertyChange` | Types | `Contracts/…`, `Listing/…` |

```shell
yarn add @cratis/showrunner.notes @cratis/arc @cratis/arc.react @cratis/fundamentals
```

`@cratis/arc`, `@cratis/arc.react` and `@cratis/fundamentals` are peer dependencies, so the application decides their versions. Use the package version that matches the NuGet packages: everything in Showrunner is released together with one version.

The proxies are generated from the C# code on a Debug build; never edit them. CI rebuilds in Debug and fails when the committed proxies differ. The collecting commands are not generated here: a builder that calls them from its own frontend generates their proxies in its own project.
