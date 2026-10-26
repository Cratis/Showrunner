---
title: Contracts
description: The public events Usher exchanges between an application and a support desk, their payloads, and how both sides subscribe to them.
---

`Cratis.Showrunner.Usher.Contracts` holds the public events an application and a support desk exchange about issues. An application gets them through `Cratis.Showrunner.Usher`, which publishes the application's side and translates the desk's side. A desk, such as Direct, references the contracts package on its own to read what applications report and to answer.

Usher records every fact about an issue as a private event in the application, in the tenant's own namespace. The private events name the actor (`ReporterFlaggedError`, `SupportAcceptedIssue`). The public events in this package name what happened to the issue (`ErrorReported`, `IssueAccepted`). A publisher turns the reporter's private facts into public events in the application's outbox. A translator turns the desk's public events back into private `Support…` facts. The [mapping](#mapping-to-private-events) at the end of this page lists every pair.

## Rules every contract follows

- **The issue id is the event source id.** Every event, in both directions, is appended under the issue's `IssueId`: a GUID in its lowercase, hyphenated `D` form, such as `8f14e45f-ceea-467a-9575-7c0a9b6b0c2d`. The id is event context, not a field in the payload. A desk answers under the same id it received.
- **Every payload carries `TenantId`.** It is the name of the tenant's Chronicle namespace in the application. A desk serves many tenants, and the application's translator appends each desk event in the namespace that `TenantId` names.
- **Nothing is nullable.** An optional collection is empty, and a value that is not known is the concept's `NotSet`, an empty string. A part of a `SliceLocation` that Arc cannot resolve is `NotSet`, not missing.
- **No timestamps that repeat the event's own time.** When something happened is the event's `Occurred` in its context. On a Chronicle kernel 19.22.1 or later, a forwarded event keeps the time it was recorded as `Occurred` in the receiving inbox. The only time in a payload is `ValidUntil` on `AccessGranted`, which is a business deadline, not the time of the event.
- **Every event is `[Public]`, and none names an origin store.** There is no `[EventStore]` attribute on any contract or on the assembly, because store names vary by deployment. Both sides subscribe explicitly; see [Subscribe to the other side](#subscribe-to-the-other-side).

Unless noted, the concepts are `ConceptAs<string>`. Namespaces are relative to `Cratis.Showrunner.Usher.Contracts`; `TenantId` and `IssueId` sit in the root namespace.

## Application to desk

The application publishes these from its outbox. They describe what the reporter did, and the expiry of a grant.

| Event | Namespace | Payload |
| --- | --- | --- |
| `ErrorReported` | `Reporting` | `TenantId: TenantId`; `Location: SliceLocation`; `CorrelationIds: IEnumerable<CorrelationId>`; `StackFrames: IEnumerable<StackFrame>`; `ErrorMessage: ErrorMessage`; `ErrorType: ErrorType`; `Fingerprint: ErrorFingerprint`; `ApplicationName: ApplicationName`; `ApplicationVersion: ApplicationVersion` |
| `IssueReported` | `Reporting` | `TenantId: TenantId`; `Location: SliceLocation`; `CorrelationIds: IEnumerable<CorrelationId>`; `Summary: Summary`; `Description: Description`; `ApplicationName: ApplicationName`; `ApplicationVersion: ApplicationVersion` |
| `IssueAttributedToReporter` | `Identity` | `TenantId: TenantId`; `Reporter: Reporter` |
| `DetailsAdded` | `Conversation` | `TenantId: TenantId`; `Details: Details` |
| `QuestionAnswered` | `Conversation` | `TenantId: TenantId`; `QuestionId: QuestionId`; `Answer: Answer` |
| `IssueReopened` | `Lifecycle` | `TenantId: TenantId`; `Reason: ReopenReason` |
| `IssueWithdrawn` | `Lifecycle` | `TenantId: TenantId` |
| `AccessGranted` | `Access` | `TenantId: TenantId`; `ValidUntil: ValidUntil` |
| `AccessRevoked` | `Access` | `TenantId: TenantId` |
| `AccessExpired` | `Access` | `TenantId: TenantId` |

- An issue starts with `ErrorReported` or `IssueReported`. What the reporter writes alongside an error report follows as `DetailsAdded`.
- `CorrelationIds` holds the correlation ids of the failing command or query and of those run just before it in the reporter's view. It can be empty. `StackFrames` is empty when the error had no stack trace, which is usual for errors raised from the frontend.
- `Fingerprint` is computed by the application, so the same error in the same slice and version always gives the same value. A desk uses it to group duplicates and otherwise treats it as opaque.
- `IssueAttributedToReporter` is published only if the reporter chooses to share who they are. Until then the desk knows the issue but not the person. Granting access always shares identity first, so `IssueAttributedToReporter` comes before `AccessGranted` on the same issue.
- `AccessExpired` is published when `ValidUntil` passes without the grant being revoked.

## Desk to application

The desk publishes these from its outbox. They describe what support did with the issue.

| Event | Namespace | Payload |
| --- | --- | --- |
| `IssueTriaged` | `Lifecycle` | `TenantId: TenantId` |
| `IssueAccepted` | `Lifecycle` | `TenantId: TenantId` |
| `IssueClosedAsWontFix` | `Lifecycle` | `TenantId: TenantId`; `Reason: WontFixReason` |
| `QuestionAsked` | `Conversation` | `TenantId: TenantId`; `QuestionId: QuestionId`; `Question: QuestionText`; `Author: SupportAuthor` |
| `ReplySent` | `Conversation` | `TenantId: TenantId`; `Reply: ReplyText`; `Author: SupportAuthor` |
| `IssueLinkedToKnownIssue` | `KnownIssues` | `TenantId: TenantId`; `KnownIssueId: KnownIssueId`; `Title: KnownIssueTitle` |
| `IssueUnlinkedFromKnownIssue` | `KnownIssues` | `TenantId: TenantId`; `KnownIssueId: KnownIssueId` |
| `IssueLinkedToWorkItem` | `WorkItems` | `TenantId: TenantId`; `WorkItem: WorkItem` |
| `IssueUnlinkedFromWorkItem` | `WorkItems` | `TenantId: TenantId`; `WorkItemReference: WorkItemReference` |
| `WorkStarted` | `Lifecycle` | `TenantId: TenantId` |
| `IssueResolved` | `Lifecycle` | `TenantId: TenantId`; `Resolution: Resolution` |
| `IssueFixedInRelease` | `WorkItems` | `TenantId: TenantId`; `ReleaseVersion: ReleaseVersion`; `WorkItemReference: WorkItemReference` |
| `AccessRequested` | `Access` | `TenantId: TenantId`; `Reason: AccessRequestReason` |

- A triaged issue is decided next: `IssueAccepted` or `IssueClosedAsWontFix`.
- The desk mints the `QuestionId` with `QuestionAsked`. The reporter's `QuestionAnswered` carries it back, so every answer targets one question.
- `QuestionAsked` and `ReplySent` carry their `Author`, because the desk's own causation does not cross into the application. The application records a translated event under its own causation, so the payload is the only place the reporter's view can learn who wrote. The desk decides what the reporter sees: a name, or `SupportAuthorName.NotSet` when it hides the names of its people, which the application shows as "Support".
- A known issue is the desk's group of duplicate reports across tenants. Each report stays its own issue in its own tenant; the application only sees the known issue's id and title.
- `IssueResolved` carries only the resolution note. A fix is often resolved when merged and released later, so the release that ships it is its own event: `IssueFixedInRelease` names the version and the work item the release closed.
- `AccessRequested` asks the reporter for a grant, and the reporter answers with `AccessGranted` or not at all. Support sessions under a grant are recorded by the application itself and have no public event.

## Shared types

| Type | Namespace | Shape |
| --- | --- | --- |
| `TenantId` | root | The tenant's namespace name |
| `IssueId` | root | `EventSourceId<Guid>`; the event source id, never in a payload |
| `SliceLocation` | `Reporting` | `Module: ModuleName`; `Feature: FeatureName`; `Slice: SliceName`. `SliceLocation.NotSet` has every part empty. |
| `CorrelationId` | `Reporting` | `ConceptAs<Guid>`. Usher's own concept, separate from `Cratis.Execution.CorrelationId`: it is a value the reporter's application ran, not the correlation of the Usher event. The two convert through their `Guid`. |
| `StackFrame` | `Reporting` | `Method: StackFrameMethod`; `File: StackFrameFile`; `Line: StackFrameLine` (`ConceptAs<int>`, one-based, `0` when not reported). Top frame first. |
| `ErrorFingerprint`, `ErrorMessage`, `ErrorType`, `ApplicationName`, `ApplicationVersion`, `Summary`, `Description` | `Reporting` | Text |
| `Reporter` | `Identity` | `IdentityProvider: IdentityProviderName`; `IdentityProviderSubject: IdentityProviderSubject`; `Name: ReporterName` (`[PII]`); `Email: Email` (`[PII]`) |
| `QuestionId` | `Conversation` | `ConceptAs<Guid>` |
| `SupportAuthor` | `Conversation` | `Name: SupportAuthorName`; `Kind: SupportAuthorKind` (`Person = 0`, `Agent = 1`) |
| `Details`, `Answer`, `QuestionText`, `ReplyText` | `Conversation` | Text |
| `ReopenReason`, `WontFixReason`, `Resolution` | `Lifecycle` | Text |
| `KnownIssueId` | `KnownIssues` | `ConceptAs<Guid>` |
| `KnownIssueTitle` | `KnownIssues` | Text |
| `WorkItem` | `WorkItems` | `Tracker: Tracker`; `Reference: WorkItemReference`; `Title: WorkItemTitle`; `Url: WorkItemUrl` (`NotSet` when the reporter cannot open it) |
| `ReleaseVersion` | `WorkItems` | Text, for example `4.12.3` |
| `ValidUntil` | `Access` | `ConceptAs<DateTimeOffset>`; the fixed end of a grant |
| `AccessRequestReason` | `Access` | Text |

`IdentityProviderName` and `IdentityProviderSubject` together identify the reporter and are the keys both sides correlate by, so they are not classified as personal data. `ReporterName` and `Email` are `[PII]`. The compliance subject of `IssueAttributedToReporter` is its event source, the issue, because each issue belongs to exactly one reporter; erasing a reporter means erasing the subject of every issue they reported. The free-text concepts are not classified. `SupportAuthorName` is not classified either: a desk that treats the names of its people as personal data sends `NotSet`.

## Mapping to private events

:::note
The private events are in `Cratis.Showrunner.Usher`; see [Backend](./backend.md#private-events). How the publisher and the translator implement these tables is in [Exchanging events with a desk](./desk-exchange.md).
:::

The publisher adds `TenantId` to every public event and publishes the reporter only after `ReporterSharedIdentity`.

| Private event in the application | Public event to the desk |
| --- | --- |
| `ReporterFlaggedError` | `ErrorReported` |
| `ReporterDescribedIssue` | `IssueReported` |
| `ReporterSharedIdentity` | `IssueAttributedToReporter` |
| `ReporterAddedDetails` | `DetailsAdded` |
| `ReporterAnsweredQuestion` | `QuestionAnswered` |
| `ReporterReopenedIssue` | `IssueReopened` |
| `ReporterWithdrewIssue` | `IssueWithdrawn` |
| `ReporterGrantedAccess` | `AccessGranted` |
| `ReporterRevokedAccess` | `AccessRevoked` |
| `AccessGrantLapsed` | `AccessExpired` |

The translator appends each desk event under the same issue id, in the namespace that `TenantId` names. It ignores an issue id it does not know, and a redelivered event changes nothing.

| Public event from the desk | Private event in the application |
| --- | --- |
| `IssueTriaged` | `SupportTriagedIssue` |
| `IssueAccepted` | `SupportAcceptedIssue` |
| `IssueClosedAsWontFix` | `SupportDeclinedToFix` |
| `QuestionAsked` | `SupportAskedQuestion` |
| `ReplySent` | `SupportReplied` |
| `IssueLinkedToKnownIssue` | `SupportLinkedIssueToKnownIssue` |
| `IssueUnlinkedFromKnownIssue` | `SupportUnlinkedIssue` |
| `IssueLinkedToWorkItem` | `SupportLinkedIssueToWorkItem` |
| `IssueUnlinkedFromWorkItem` | `SupportUnlinkedWorkItem` |
| `WorkStarted` | `SupportStartedWork` |
| `IssueResolved` | `SupportResolvedIssue` |
| `IssueFixedInRelease` | `ReleaseShippedFix` |
| `AccessRequested` | `SupportRequestedAccess` |

`SupportSignedInAsReporter` and `SupportSessionEnded` have no public event. The application records them itself when support signs in on the reporter's behalf, with both identities in the causation.

## Subscribe to the other side

Nothing in the contracts package routes an observer to the other side. Each side keeps the name of the other side's store in its own configuration and does two things at startup:

1. Subscribe its own store to the other store's outbox. Chronicle forwards the listed event types into `inbox-{other store}` on the subscribing store, in the same namespace. Use the other store's name as the subscription id, and filter the union of the event types its consumers need.
2. Observe `inbox-{other store}` with observers that name that event sequence. An observer that names no sequence reads the store's own event log and never sees the other side's events.

An application gets this from `Cratis.Showrunner.Usher`: it subscribes to each store in `DeskStores` (`Direct` by default) for the desk-to-application events, and its translator observes `inbox-{desk store}`. Chronicle forwards only into namespaces the subscribing store has, so a desk opens its store in each tenant namespace of the application; see [Setting up a desk](./desk-exchange.md#setting-up-a-desk). A desk does the same for each application store it serves:

```csharp
await store.Subscriptions.Subscribe(
    new EventStoreSubscriptionId(applicationStore),
    applicationStore,
    subscription => subscription
        .WithEventType<ErrorReported>()
        .WithEventType<IssueReported>()
        .WithEventType<IssueAttributedToReporter>()
        .WithEventType<DetailsAdded>()
        .WithEventType<QuestionAnswered>()
        .WithEventType<IssueReopened>()
        .WithEventType<IssueWithdrawn>()
        .WithEventType<AccessGranted>()
        .WithEventType<AccessRevoked>()
        .WithEventType<AccessExpired>());
```

Here `store` is the desk's own `IEventStore` and `applicationStore` is the configured name of the application's store. Projections, reducers and reactors over the application's events then name the `inbox-{application store}` sequence, for example `[Reactor(eventSequence: "inbox-Ledger")]` for an application store named `Ledger`. Consumers on both sides must tolerate the same event arriving twice, because the forwarder can deliver it again.
