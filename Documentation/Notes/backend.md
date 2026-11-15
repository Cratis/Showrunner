---
title: Backend
description: How to add Notes to an application, the commands and queries it gives the user, the private events it records, and the options that switch it off.
---

`Cratis.Showrunner.Notes` gives an application's users the backend for product feedback: commands to give, suggest and withdraw notes, and a live query for the list of their notes with the status of each. Everything is recorded as private events in the application's own event store, in the tenant's namespace.

## Add Notes to an application

Reference the `Cratis.Showrunner.Notes` package and call `AddNotes` on the host builder, next to `AddCratis`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddNotes(notes =>
{
    notes.ApplicationName = "Ledger";
    notes.PublicationId = "pub-7f3a";
    notes.CollectingStores = ["Studio"];
});
```

The same is available on the service collection as `services.AddNotes(...)`.

| Option | Default | Meaning |
| --- | --- | --- |
| `Enabled` | `true` | Whether Notes is switched on. Off, every command is refused, the availability query says so and the React button does not render. |
| `ApplicationName` | none, required | The name of the application as the receiving side sees it on every note. |
| `PublicationId` | none, required | The publication the application runs as: how the receiving side finds the organization, application and event model. The builder of the application assigns it when it publishes the application. |
| `ApplicationVersion` | the entry assembly's informational version, without build metadata | The version published with every note. |
| `AccessMode` | `Everyone` | Who may give feedback. `Everyone` lets every signed-in user, as before access could be controlled. `GrantedUsers` lets only the users a desk granted the right to. See [Access](./access.md). |
| `AccessStores` | `["Direct"]` | `GrantedUsers` only. The event stores of the desks that decide who may give feedback; Notes subscribes to their outbox. |
| `EventStore` | the application's own event store | The event store Notes records the application's facts in. Notes uses the event store of the application's `AddCratis` or `AddCratisChronicle` setup; set it only to record in another one. |
| `CollectingStores` | `["Studio"]` | The event stores of the receiving sides the application publishes to and receives answers from. |
| `Audience`, `CollectorAudience` | unset | The audience that identifies this application to the collecting stores (filters the subscription and is sent on every note, so the collector addresses its answers back), and the audience of the collector (addresses published notes). See [Audience routing](../BoxOffice/audience-routing.md). |
| `PollingInterval` | one minute | How often Notes opens tenant namespaces it does not serve yet and makes sure the application is subscribed to the collecting stores. |
| `Transport` | `SameKernel` | How notes reach the receiving side: `SameKernel` (Chronicle's outbox, subscription and inbox) or `Http`. See [Transports](./transports.md). |
| `CollectorUrl` | none | `Http` only. The absolute `http(s)` URL of the collector. |
| `CollectorToken` | none | `Http` only. The bearer secret of the publication. Prefer `CollectorTokenFile`. |
| `CollectorTokenFile` | none | `Http` only. The path of a file, typically a mounted secret, that holds the bearer secret. Read again on every use; wins over `CollectorToken`. |
| `CollectorTenantId` | none | `Http` only. The tenant the collector knows the publication's notes by; every published note names it instead of its namespace. |
| `RelayInterval` | five seconds | `Http` only. How often the relay looks for notes to send and answers to apply. |
| `RelayBatchSize` | 100 | `Http` only. The most notes sent in one request. |

The host fails to start when `ApplicationName` or `PublicationId` is not set, when `CollectingStores` is empty on the `SameKernel` transport, or when the `Http` transport has no valid `CollectorUrl` or no secret, unless `Enabled` is `false`. It also fails when `AccessMode` is `GrantedUsers` and `AccessStores` is empty or the transport is `Http`: a desk's decisions reach the application through Chronicle's outbox and inbox, and Notes does not guess that nobody may give feedback. Arc discovers Notes' commands and queries, and Chronicle its events, read models and reactors, from the referenced assembly; there is nothing else to register. `AddNotes` also adds a hosted service that opens every tenant namespace, observes the inbox of each collecting store and subscribes the application to it; see [Collecting](./collecting.md#what-the-application-needs).

Every command and query requires a signed-in user. Notes takes who the giver is from the signed-in identity, never from the request: the `sub` claim (or the name identifier) is the user's id and the `name` claim their display name. There is no giver field on any command, and a call that is not signed in is refused.

## Commands

| Slice | Command | What it records |
| --- | --- | --- |
| `Giving/GiveNoteOnContext` | `GiveNoteOnContext(NoteId, Context, Kind, Text)` | `GiverGaveNoteOnContext` |
| `Giving/GiveNoteOnSlices` | `GiveNoteOnSlices(NoteId, Context, Slices, Kind, Text)` | `GiverGaveNoteOnSlices` |
| `Giving/GiveNoteOnScreen` | `GiveNoteOnScreen(NoteId, Context, Kind, Text)` | `GiverGaveNoteOnScreen` |
| `Giving/GiveNoteOnComponent` | `GiveNoteOnComponent(NoteId, Context, Component, Kind, Text)` | `GiverGaveNoteOnComponent` |
| `Giving/SuggestComponentPropertyChange` | `SuggestComponentPropertyChange(NoteId, Context, Component, Property, CurrentValue, SuggestedValue)` | `GiverSuggestedComponentPropertyChange` |
| `Withdrawing/WithdrawNote` | `WithdrawNote(NoteId)` | `GiverWithdrewNote` |

The caller chooses the `NoteId` of a new note, so a retried command does not give a second note. The suggestions of one picking session and the text about the component share one `NoteId`; there is one `SuggestComponentPropertyChange` per property.

The commands refuse, with a validation error:

- everything, while Notes is switched off;
- a note whose id is already given, unless it is the user's own note that has only suggested property changes so far and the command is `GiveNoteOnComponent` (or another suggestion);
- a note that belongs to someone else, which is indistinguishable from one that does not exist;
- a missing context, an empty text, a text over 4000 characters, no slice for `GiveNoteOnSlices`, a missing component or property, and more than 50 slices, model references or ancestors (`NoteLimits`);
- a change to a withdrawn note, and withdrawing a note twice;
- giving or suggesting, when `AccessMode` is `GrantedUsers` and the desk has not granted the user the right or has revoked it. The refusal is the same message in both cases and reveals nothing about what the desk decided. Withdrawing a note and `MyNotes` stay available to whoever gave a note.

Everything a user sends is stored, published and shown to the people building the application, so every text and list is bounded.

## Queries

| Query | On | What it returns |
| --- | --- | --- |
| `MyNotes` (observable) | `MyNote` | The signed-in user's notes in the current tenant, most recent activity first, again every time one changes. Each note has its `Status`: `Given`, `Acknowledged`, `Addressed`, `Declined` or `Withdrawn`, and a `StatusDetail` with the version that addresses it or the reason it is declined. |
| `Availability` | `NotesAvailability` | `IsEnabled`: whether Notes is available to the user asking. It is whether Notes is switched on and, with `GrantedUsers`, whether the desk granted the user the right. It needs no sign-in, so the feedback button can ask before anyone has signed in; anyone not signed in is told no when only granted users may give feedback. |

`MyNotes` returns nothing to a user who is not signed in and never another user's notes. A note only moves forward: a withdrawn note stays withdrawn, and an acknowledgement that arrives after a resolution does not take it back. An answer about a note the application never recorded changes nothing.

## Private events

| Event | Recorded when |
| --- | --- |
| `GiverGaveNoteOnContext`, `GiverGaveNoteOnSlices`, `GiverGaveNoteOnScreen`, `GiverGaveNoteOnComponent` | The user gives a note. Each carries the application, the giver, the context, the kind and the text. |
| `GiverSuggestedComponentPropertyChange` | The user suggests a new value for a property of a component. |
| `GiverWithdrewNote` | The user withdraws the note. |
| `ReceiverAcknowledgedNote`, `ReceiverAddressedNote`, `ReceiverDeclinedNote` | The receiving side's answers, translated from the public events. |
| `DeskGrantedFeedbackAccess`, `DeskRevokedFeedbackAccess` | `GrantedUsers` only. The desk's decisions about one user, translated from the public events and recorded under the user's own id. |

The user's facts are appended with the user as compliance subject. The outbox publisher turns each into its public event; see [Contracts](./contracts.md#mapping-to-private-events).

## Publishing to the receiving side

The `Publishing/OutboxPublisher` reactor appends each public event to the outbox of the same namespace under the same note id. It names the namespace as the `TenantId` and keeps the private fact's correlation id, compliance subject and the time it happened. Public events go only to the outbox, never to the event log.

Chronicle delivers an event to a reactor at least once. Each publication is appended with an idempotency key derived from the delivery, so a repeated delivery finds its publication already in the outbox and appends nothing. The reactor is excluded from replay, so replaying it does not try to publish history again. A newly deployed publisher does catch up: it publishes everything the namespace recorded before it ran.

### Configuring Notes from the environment

`AddNotes` takes a delegate, so the host decides where the values come from. Binding a configuration section gives the usual environment variable forms; the section name is the host's choice, and the documentation and Studio use `Notes`:

```csharp
builder.AddNotes(notes => notes.BindFrom(builder.Configuration.GetSection("Notes")));
```

```text
Notes__ApplicationName=Ledger
Notes__PublicationId=pub-7f3a
Notes__Transport=Http
Notes__CollectorUrl=https://studio.internal:8443
Notes__CollectorTokenFile=/var/run/secrets/notes/token
Notes__CollectorTenantId=acme-corp
```

## Asking whether a user may give feedback

A host that gates something of its own on the same decision, such as a menu entry or a page, injects `IFeedbackAccess`:

```csharp
public class Menu(IFeedbackAccess feedback)
{
    public Task<bool> ShowsFeedback() => feedback.MayGiveFeedback();
}
```

`MayGiveFeedback()` answers for the signed-in user and `MayGiveFeedback(FeedbackGiverIdentity)` for a named one. Both are advisory reads of the tenant's current state and are always `true` when everyone may give feedback. The commands do not use them: they decide with a read that guards the append, so a revocation that lands before a note is appended refuses the note. See [Access](./access.md).

## Receiving the answers

The `Receiving/ReceiverEventTranslator` observes `inbox-{collecting store}` in every tenant namespace and records each answer as its private fact in the namespace its `TenantId` names. A tenant that does not exist or is ambiguous, an event that names no tenant and a note that was never given in the tenant are logged and skipped; they change nothing, and failing on them would hold up later events. The tenant is matched to a namespace exactly, case included. Every copy of the same answer yields one fact, because the private fact is appended with an idempotency key derived from the answer itself.

With the `Http` transport the answers arrive through the collector instead of an inbox; see [Transports](./transports.md#the-http-transport).
