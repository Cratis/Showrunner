---
title: Collecting
description: The receiving side of Notes, hosted by the builder of an application - how it reads the application's outbox, records each note in the tenant's namespace, answers it, and what the receiving system has to configure.
---

`Cratis.Showrunner.Notes.Collecting` is the receiving side. The builder of an application, such as Studio, hosts it in its own Arc and Chronicle process. It reads the notes the application publishes, records each note as a private fact in the namespace its `TenantId` names, and offers commands to acknowledge, address and decline a note that publish the answers back.

Collecting does not depend on the application's backend package. It depends only on the [contracts](./contracts.md).

## Add collecting to a host

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddNoteCollecting(collecting =>
{
    collecting.AppStores = ["Ledger"];
});
```

The same is available on the service collection as `services.AddNoteCollecting(...)`.

| Option | Default | Meaning |
| --- | --- | --- |
| `AppStores` | none | The names of the event stores of the applications whose notes are collected. |
| `PollingInterval` | one minute | How often collecting opens the namespaces it does not serve yet and makes sure it is subscribed to the application stores. |

A builder that publishes applications while it runs registers its own `IAppStores`, which returns the names of the stores of the applications published so far. It is asked again every round, so a new application is subscribed to within one `PollingInterval`. Without one, `AppStores` names them.

## What the receiving system has to configure

1. **For the same-kernel transport, the application's event store must be reachable as an event store of the same Chronicle kernel the collecting host connects to.** Chronicle forwards events between event stores through a subscription: the collecting store subscribes to the application's store by name, and Chronicle forwards the listed event types from that store's outbox into `inbox-{application store}` on the collecting store. A subscription is between two event stores of one Chronicle server. A published application that talks to its own, separate Chronicle server can not be subscribed to from another server, so the application's event store has to be on the kernel the collecting host uses, so an application with its own kernel uses the [HTTP transport](./transports.md#the-http-transport) and the endpoints below.
2. **The name of the application's event store**, as the application names it in its Chronicle connection (`ChronicleClientOptions.EventStore`), in `AppStores` or in the `IAppStores` the host supplies.
3. **A namespace in the collecting store named exactly as each tenant.** The application names its tenant in `TenantId`, which is the name of the tenant's namespace in the application. Chronicle forwards an event only into the namespaces the subscribing store has, and the translator records the note in the namespace `TenantId` names, matched exactly, case included. The collecting host opens every namespace its store has, so creating the namespace for an organization is enough. A tenant without a namespace gets no notes: Chronicle does not forward into it, and a note that arrives in another namespace but names a tenant the store does not have is logged and skipped.
4. **The application has to know the collecting store's name** in `NotesOptions.CollectingStores` (default `Studio`) and be subscribed to its outbox. That subscription is the application's own, made by `AddNotes`; the collecting store has to be in the same Chronicle kernel for it too.
5. **Who may answer.** The commands require a signed-in person. Add the roles or policies the builder needs through Arc authorization for the host.

## Collecting over HTTP

An application that runs its own Chronicle kernel relays its notes to the host over HTTP. The host maps the endpoints and says how a publication is authenticated.

```csharp
builder.Services.AddNoteCollecting(collecting => { });
builder.Services.AddSingleton<IResolveNotesPublications, MyPublications>();

var app = builder.Build();
app.MapNoteCollecting();
```

`IResolveNotesPublications` is the host's authentication:

```csharp
public interface IResolveNotesPublications
{
    Task<NotesPublication?> Resolve(PublicationId publicationId, string bearerToken);
}

public record NotesPublication(TenantId TenantId, AppStoreName AppStore);
```

- `TenantId` is the name of the namespace in the collecting store the publication's notes are recorded in.
- `AppStore` is recorded as `AppStore` on every received fact. It does not have to be an event store of this kernel.
- Return `null` for an unknown publication and for a wrong secret alike. Store a hash of each secret and compare in constant time, for example with `CryptographicOperations.FixedTimeEquals` over the hashes of the stored and the presented secret.

`MapNoteCollecting` throws when the host did not register an `IResolveNotesPublications`. See [Transports](./transports.md#the-collector-endpoints) for the endpoints, their payloads, their status codes and the security model. The endpoints do not use the host's user authentication: map them only on an internal address.

## Background work

Collecting adds a hosted service that runs a round of work when the host starts and then every `PollingInterval`. Each round:

1. Opens the collecting store in the default namespace and in every namespace it has. Chronicle runs the host's observers only in namespaces the host has opened.
2. Observes `inbox-{application store}` of each application store in every one of those namespaces.
3. Subscribes the collecting store to the outbox of each application store, filtered to the six note events, under the subscription id `notes-{application store}`.

Everything a round acts on is stored in Chronicle, so a restart loses nothing: the first round after it catches up. A round that fails is logged as a warning and the next round tries again.

## Receiving a note

A translator per application store observes the inbox and records each public event as its private fact:

- It appends the private fact to the event log of the namespace the event's `TenantId` names, under the same note id.
- The private fact keeps the time the application recorded the event as its `Occurred`, its correlation id and the giver's compliance subject, so erasing the giver erases what they wrote here too.
- The private fact names the application store it came from (`AppStore`).
- A note arriving for a namespace the collecting store does not have or can not tell apart, and an event that names no tenant, are logged as a warning and skipped. They change nothing, and failing on them would hold up the application's later events.
- **Redelivery does not duplicate notes.** The forwarder can put the same event into the inbox twice, and the translator can be handed the same inbox event again after a failure. The private fact is appended with an idempotency key derived from the event itself (the application store, the note, the event type, when it occurred, its correlation id and its content), so every copy records one fact. Two events the application recorded separately are both recorded, even when their content is the same.
- **A withdrawal is honored.** `NoteWithdrawn` is recorded as `NoteWithdrawalReceived`, and the commands refuse to answer a withdrawn note. A withdrawal for a note that was never received is logged and skipped.

The translator is not replayable: replaying an inbox would only translate events that are already recorded.

The private events are public types of the collecting package: `NoteOnContextReceived`, `NoteOnSlicesReceived`, `NoteOnScreenReceived`, `NoteOnComponentReceived`, `ComponentPropertyChangeReceived` and `NoteWithdrawalReceived`. The host builds the views and reactors it needs on them, for example to find the organization, application and event model a note is about from its `Application.PublicationId`.

## Answering a note

| Slice | Command | What it records | Published as |
| --- | --- | --- | --- |
| `Responding/AcknowledgeNote` | `AcknowledgeNote(NoteId)` | `OperatorAcknowledgedNote` | `NoteAcknowledged` |
| `Responding/AddressNote` | `AddressNote(NoteId, Version)` | `OperatorAddressedNote` | `NoteAddressed` |
| `Responding/DeclineNote` | `DeclineNote(NoteId, Reason)` | `OperatorDeclinedNote` | `NoteDeclined` |

The commands run in the namespace of the request's tenant, and require a signed-in person. They refuse a note that was never received in the tenant, a note the giver withdrew, a note that is already acknowledged or declined, a note already addressed in the same version, an empty version and an empty reason. Addressing a note in a later version is allowed.

The `Publishing/ResponsePublisher` reactor turns each into its public event in the outbox of the namespace it was recorded in, which is the tenant the application routes it back to. As in the application, the append carries an idempotency key derived from the delivery and the reactor does not replay, so a repeated delivery publishes once.
