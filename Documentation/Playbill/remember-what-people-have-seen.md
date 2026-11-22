---
title: Remember what people have seen
description: The MarkReleaseNotesSeen command, the ReleaseNotesStatus query and the options Playbill uses to tell each person which releases are new, so a what's new dialog is shown once on every device.
---
Which releases a person has been told about is recorded in the application's own Chronicle event store, per person, so it follows them to every device. It needs the application to be an Arc application with Chronicle; the endpoints that serve the history do not.

## The command

`MarkReleaseNotesSeen` records that the signed-in person has seen the notes up to a version. The generated proxy is in `@cratis/showrunner.playbill`.

```tsx
const [mark] = MarkReleaseNotesSeen.use();
mark.version = '2.3.0';
await mark.execute();
```

It only moves forward:

| The person has seen | They mark | Result |
| --- | --- | --- |
| nothing | `2.3.0` | `ReleaseNotesSeen` is appended. |
| `2.1.0` | `2.3.0` | `ReleaseNotesSeen` is appended. |
| `2.3.0` | `2.3.0` | Nothing is appended. |
| `2.3.0` | `2.1.0` | Nothing is appended: seeing an older release again never moves them back. |

It is refused, and appends nothing, when:

- the version is not a version number,
- the application has no note for it, or
- the caller is not signed in.

Seeing `2.3.0` means having seen every release up to it. A person is not asked about `2.2.0` after they have seen `2.3.0`.

## The query

`ReleaseNotesStatus.ForMe` is an observable query of the signed-in person's status:

| Property | Meaning |
| --- | --- |
| `current` | The version the application is running; empty when it is not known. |
| `lastSeen` | The newest release the person has seen; empty when they have seen none. |
| `unread` | How many releases are newer than that. |
| `unreadVersions` | Those versions, newest first. |

It emits one status and a new one whenever the person marks a release as seen, so a badge clears on every device they have open. For someone who is not signed in it is refused. A person who has seen nothing has the newest release unread, and only that one: the history before they arrived is not news to them.

In React, `usePlaybillStatus` wraps both:

```tsx
const { unread, unreadVersions, markSeen } = usePlaybillStatus();
```

## The event

`ReleaseNotesSeen` is appended under a stream of the person's own. See the [Event reference](./event-reference.md).

## Options

`PlaybillOptions` is configured with `AddPlaybill`:

| Option | Default | Meaning |
| --- | --- | --- |
| `Assemblies` | the entry assembly | The assemblies that hold the embedded release notes. |
| `CurrentVersion` | the assembly's informational version, without build metadata | The version shown as current. |
| `AllowAnonymous` | `false` | Let callers who are not signed in read the endpoints. |

The person is identified by the `sub` claim, or the name identifier when there is no `sub`. An application that identifies people differently registers its own `ISignedInViewer`.
