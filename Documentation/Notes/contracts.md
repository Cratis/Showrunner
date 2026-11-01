---
title: Contracts
description: The public events Notes exchanges between an application and the people building it, their payloads, and how both sides subscribe to them.
---

`Cratis.Showrunner.Notes.Contracts` holds the public events an application and the receiving side exchange about notes. An application gets them through `Cratis.Showrunner.Notes`, which publishes the application's side and translates the receiving side's answers. The builder of the application gets them through `Cratis.Showrunner.Notes.Collecting`, which receives the notes and publishes the answers. Either side can also reference the contracts package on its own.

Notes records every fact about a note as a private event in the application, in the tenant's own namespace. The private events name the actor (`GiverGaveNoteOnContext`, `ReceiverAddressedNote`). The public events in this package name what happened to the note (`NoteOnContextGiven`, `NoteAddressed`). The [mapping](#mapping-to-private-events) at the end of this page lists every pair.

## Rules every contract follows

- **The note id is the event source id.** Every event, in both directions, is appended under the note's `NoteId`: a GUID in its lowercase, hyphenated `D` form. The id is event context, not a field in the payload. The receiving side answers under the same id it received.
- **Every payload carries `TenantId`.** It is the name of the tenant's Chronicle namespace in the application. The receiving side serves many applications and tenants, records each note in the namespace that `TenantId` names, and publishes its answer in that namespace so the application routes it back to the tenant.
- **Nothing is nullable.** An optional collection is empty, and a value that is not known is the concept's `NotSet`, an empty string. No dialog is `NoteDialog.NoDialog`; a screen that could not be resolved is `NoteScreen.NotSet`.
- **No timestamps that repeat the event's own time.** When something happened is the event's `Occurred` in its context.
- **Every event is `[Public]`, and none names an origin store.** There is no `[EventStore]` attribute on any contract or on the assembly, because store names vary by deployment. Both sides subscribe explicitly; see [Subscribe to the other side](#subscribe-to-the-other-side).
- **The giver is the compliance subject.** Every fact a user records is appended with the subject `notes-giver-<sha-256 of the user id>`, and the publisher publishes each contract event with the subject of the private event it comes from. One erasure of that subject therefore covers everything the person wrote on all of their notes. The subject is a one-way hash because it travels unencrypted with every event.

Namespaces are relative to `Cratis.Showrunner.Notes.Contracts`; `TenantId` and `NoteId` sit in the root namespace. `SliceLocation`, `ApplicationName`, `ApplicationVersion` and `CorrelationId` are Usher's, in `Cratis.Showrunner.Usher.Contracts.Reporting`: Notes reuses them rather than defining a second place in the application, so both parts describe a place and an application the same way. This is a project reference, so referencing the Notes contracts brings in the Usher contracts package; the Usher events are not used unless the host adds Usher.

## Application to receiving side

The application publishes these from its outbox.

| Event | Namespace | Payload |
| --- | --- | --- |
| `NoteOnContextGiven` | `Giving` | `TenantId`, `Application: PublishedApplication`, `Giver: NoteGiver`, `Context: NoteContext`, `Kind: NoteKind`, `Text: NoteText` |
| `NoteOnSlicesGiven` | `Giving` | `TenantId`, `Application`, `Giver`, `Context`, `Slices: IEnumerable<SliceLocation>`, `Kind`, `Text` |
| `NoteOnScreenGiven` | `Giving` | `TenantId`, `Application`, `Giver`, `Context`, `Kind`, `Text` |
| `NoteOnComponentGiven` | `Giving` | `TenantId`, `Application`, `Giver`, `Context`, `Component: PickedComponent`, `Kind`, `Text` |
| `ComponentPropertyChangeSuggested` | `Components` | `TenantId`, `Application`, `Giver`, `Context`, `Component`, `Property: ComponentProperty`, `CurrentValue: PropertyValue`, `SuggestedValue: PropertyValue` |
| `NoteWithdrawn` | `Giving` | `TenantId` |

- Each is a distinct fact. `NoteOnContextGiven` is the default action of the feedback button; `NoteOnSlicesGiven` is for slices the user explicitly picked; `NoteOnScreenGiven` is for the screen picked as the subject, as opposed to the context it happens to be in.
- `NoteOnComponentGiven` and the `ComponentPropertyChangeSuggested` events of the same picking session share the `NoteId`. There is one `ComponentPropertyChangeSuggested` event per property, and a session can suggest changes without writing text.
- `NoteWithdrawn` ends the note: the receiving side stops acting on it.

## Receiving side to application

The receiving side publishes these from its outbox, so the giver can see what happened.

| Event | Namespace | Payload |
| --- | --- | --- |
| `NoteAcknowledged` | `Responses` | `TenantId` |
| `NoteAddressed` | `Responses` | `TenantId`, `AddressedInVersion` |
| `NoteDeclined` | `Responses` | `TenantId`, `Reason: DeclineReason` |

`NoteAcknowledged` says the receiving side recorded the note, `NoteAddressed` that a version that acts on it was rolled out, and `NoteDeclined` that it decided not to act on it.

## Shared types

| Type | Namespace | Shape |
| --- | --- | --- |
| `NoteGiver` | `People` | `UserId: GiverUserId` (`[PII]`), `DisplayName: GiverDisplayName` (`[PII]`) |
| `PublishedApplication` | `Applications` | `ApplicationName`, `ApplicationVersion`, `PublicationId`. The `PublicationId` is how the receiving side finds the organization, application and event model a note is about. |
| `NoteContext` | `Context` | `Location: SliceLocation`, `ActiveSlices: IEnumerable<SliceLocation>`, `Screen: NoteScreen`, `Route`, `Dialog: NoteDialog`, `ModelReferences: IDictionary<string, string>`, `CorrelationId` |
| `NoteScreen` | `Context` | `ScreenId`, `ScreenName`, `ScreenPath`. `NoteScreen.NotSet` has every part empty. |
| `NoteDialog` | `Context` | `DialogTitle`, `Location: SliceLocation`. `NoteDialog.NoDialog` has an empty title and `SliceLocation.NotSet`. |
| `PickedComponent` | `Components` | `ComponentName`, `ComponentId`, `Part: ComponentPart`, `Ancestors: IEnumerable<ComponentName>`, `Location: SliceLocation` |
| `NoteKind` | `Giving` | `Problem = 0`, `Suggestion = 1`, `Question = 2`, `Praise = 3` |
| `NoteText` | `Giving` | Text the user wrote (`[PII]`) |

`ModelReferences` holds identifiers of model elements, supplied by the host and keyed by what they identify, such as the event model or the slice of the model the screen renders.

### Personal data

What the user writes, and who they are, is classified `[PII]` on the shared concepts themselves, so it is encrypted in both stores: `NoteText`, `GiverUserId` and `GiverDisplayName`. The `DeclineReason` and `AddressedInVersion` the receiving side writes are the builder's own words and are not classified.

**The receiving side reads forwarded events in the clear.** Encryption keys are per event store and namespace. When Chronicle forwards an event from the application's outbox into the receiving side's inbox, it copies the subject's key into the receiving store, in the same namespace, the first time it forwards an event of that subject. Erasure is per store: erasing the giver in the application deletes the application's key, and a receiving side that must forget them too deletes the same subject in its own store.

## Subscribe to the other side

Nothing in the contracts package routes an observer to the other side. Each side keeps the name of the other side's store in its own configuration and does two things at startup:

1. Subscribe its own store to the other store's outbox. Chronicle forwards the listed event types into `inbox-{other store}` on the subscribing store, in the same namespace. Use a subscription id of the form `notes-{other store}` and filter the event types its consumers need.
2. Observe `inbox-{other store}` with observers that name that event sequence.

An application gets this from `Cratis.Showrunner.Notes`: it subscribes to each store in `CollectingStores` for the three answers. The receiving side gets it from `Cratis.Showrunner.Notes.Collecting`: it subscribes to each application store for the six notes. Consumers on both sides must tolerate the same event arriving twice, because the forwarder can deliver it again. See [Collecting](./collecting.md) for what the receiving system has to configure.

## Mapping to private events

The publisher adds `TenantId` to every public event: the namespace the private fact was recorded in.

| Private event in the application | Public event to the receiving side |
| --- | --- |
| `GiverGaveNoteOnContext` | `NoteOnContextGiven` |
| `GiverGaveNoteOnSlices` | `NoteOnSlicesGiven` |
| `GiverGaveNoteOnScreen` | `NoteOnScreenGiven` |
| `GiverGaveNoteOnComponent` | `NoteOnComponentGiven` |
| `GiverSuggestedComponentPropertyChange` | `ComponentPropertyChangeSuggested` |
| `GiverWithdrewNote` | `NoteWithdrawn` |

The application's translator appends each answer under the same note id, in the namespace that `TenantId` names. It ignores a note it did not record, and a redelivered event changes nothing.

| Public event from the receiving side | Private event in the application |
| --- | --- |
| `NoteAcknowledged` | `ReceiverAcknowledgedNote` |
| `NoteAddressed` | `ReceiverAddressedNote` |
| `NoteDeclined` | `ReceiverDeclinedNote` |

The collecting side records the notes the same way:

| Public event from the application | Private event on the collecting side |
| --- | --- |
| `NoteOnContextGiven` | `NoteOnContextReceived` |
| `NoteOnSlicesGiven` | `NoteOnSlicesReceived` |
| `NoteOnScreenGiven` | `NoteOnScreenReceived` |
| `NoteOnComponentGiven` | `NoteOnComponentReceived` |
| `ComponentPropertyChangeSuggested` | `ComponentPropertyChangeReceived` |
| `NoteWithdrawn` | `NoteWithdrawalReceived` |

and publishes the builder's answers: `OperatorAcknowledgedNote` as `NoteAcknowledged`, `OperatorAddressedNote` as `NoteAddressed` and `OperatorDeclinedNote` as `NoteDeclined`.

## Changing a contract

Every event is guarded by `EventSchemas.snapshot.json` and `ReleasedEventSchemas.json` in the contracts project. The specification `for_EventSchemas` fails when a released schema changes, and when the snapshot is out of date for an unreleased one. Regenerate the snapshot with `NOTES_UPDATE_EVENT_SCHEMA_SNAPSHOT=1 dotnet test Source/DotNET/Notes.Contracts/Notes.Contracts.csproj --filter FullyQualifiedName~and_comparing_the_snapshot`, review the difference, and add the keys of events that were released to `ReleasedEventSchemas.json`. A released event is never changed; a new generation is added instead.
