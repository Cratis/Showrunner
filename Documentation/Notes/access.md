---
title: Access and targeting
description: Control who can give feedback with Notes, grant and revoke the right from a desk, and choose what a note is about, from a whole module to one component.
---

Notes is the one way for the users of any Cratis application to give feedback, whether the application was rendered from a model or built by hand. Two questions decide how it feels to use: who may give a note, and what a note can be about. This page answers both.

## Who can give feedback

An application chooses between two modes with `NotesOptions.AccessMode`.

| Mode | Who may give a note |
| --- | --- |
| `Everyone` (default) | Every signed-in user. Nothing about access changes. |
| `GrantedUsers` | Only the users a desk granted the right to, until it revokes it. |

```csharp
builder.AddNotes(notes =>
{
    notes.ApplicationName = "Ledger";
    notes.PublicationId = "pub-7f3a";
    notes.AccessMode = FeedbackAccessMode.GrantedUsers;
    notes.AccessStores = ["Direct"];
});
```

`AccessStores` names the event stores of the desks that decide. It defaults to `Direct`, and Notes uses it only in `GrantedUsers` mode: an application that lets everyone give notes subscribes to no desk. The mode needs the [same-kernel transport](./transports.md), because a desk's decisions arrive through Chronicle's outbox and inbox; the host refuses to start with `GrantedUsers` over HTTP or without an access store, rather than quietly refusing everyone.

### How a desk grants and revokes the right

The desk decides, the application obeys, and the two never share a database. A desk publishes two public events from its outbox, defined in the [contracts](./contracts.md#desk-to-application):

- `FeedbackAccessGranted(TenantId, Giver, GrantedBy)` grants one user of one tenant the right.
- `FeedbackAccessRevoked(TenantId, Giver, RevokedBy)` takes it away.

The user is named the way Usher names a reporter: the identity provider and the subject it issued, for example `github` and `1234`. The application reads both from the signed-in user's claims, the subject from `sub` and the provider from the identity provider claim, so both have to match; a grant for the same subject at another provider is not the user's.

The desk appends each decision under the user's `FeedbackGiverId`, the same id for a given user on both sides:

```csharp
var id = FeedbackGiverId.For(giver);           // the event source id
var subject = FeedbackGiverId.SubjectOf(giver); // the compliance subject
await outbox.Append(id, new FeedbackAccessGranted(tenant, giver, "desk-operator-1"), subject: subject);
```

The application subscribes its event store to the desk's outbox for the two events, with the subscription id `notes-access-{desk store}`, observes the inbox, and records each decision as a private fact in the tenant's namespace that `TenantId` names: `DeskGrantedFeedbackAccess` or `DeskRevokedFeedbackAccess`. A decision for a namespace the application does not have is logged and skipped. Chronicle can deliver an event twice, and a repeat changes nothing.

### How the latest decision wins

The application folds a user's decisions into one state, `FeedbackGiver`. The latest decision by the time the desk made it stands, so a grant and a revocation that arrive out of order still end the way the desk meant. When two decisions have the same time, the revocation wins. Granting after a revocation grants again. A user the desk never decided about has no state, and may not give feedback.

### What a refused user sees

- **Commands.** `GiveNoteOnContext`, `GiveNoteOnSlices`, `GiveNoteOnScreen`, `GiveNoteOnComponent` and `SuggestComponentPropertyChange` read the user's state in `Provide` with a read that guards the append: a revocation that lands before the note is appended refuses the note. The refusal is one validation message, `Giving feedback is not available to you.`, whether the user was never granted or was revoked, so it reveals nothing about what the desk decided.
- **Availability.** The `Availability` query answers `isEnabled: false` for a user who may not give feedback, and for anyone who is not signed in. The React `NotesProvider` asks again when the user signs in, so the feedback button stays hidden for a user nobody granted the right, and shows for one the desk granted, without a flash.
- **Their own notes.** `MyNotes` and `WithdrawNote` stay available to whoever gave a note: revoking the right does not take a note away from the user who wrote it.

### Asking the same question from the host

Inject `IFeedbackAccess` to gate something of your own on the same decision, for example a menu entry. `MayGiveFeedback()` answers for the signed-in user. It is an advisory read of the tenant's current state, always `true` when everyone may give feedback.

## What a note can be about

A note names where the user was with the module, feature and slice Arc resolves, the screen, the route and the open dialog, and then says what it is about. Every level has a command, and the levels do not depend on how the application was built: a hand-built application and one rendered from a model use the same commands and the same contract events.

| Level | Command | Carries |
| --- | --- | --- |
| What is on screen now | `GiveNoteOnContext` | The context only. The default action of the feedback button. |
| The screen | `GiveNoteOnScreen` | The context, with the screen as the subject. |
| A whole module | `GiveNoteOnSlices` | A location with the module and the feature and slice not set. |
| A whole feature | `GiveNoteOnSlices` | A location with the module and the feature and the slice not set. |
| One or more slices | `GiveNoteOnSlices` | One location per slice, with all three parts. |
| One component | `GiveNoteOnComponent` | The component, its part and ancestors, and the location of its slice. |
| A property of a component | `SuggestComponentPropertyChange` | The property, its current value and the value the user suggests. |

A module or a feature needs no event of its own. A `SliceLocation` has a module, a feature and a slice, and a part that cannot be resolved is its `NotSet`, never missing. A whole module is therefore the location whose feature and slice are `NotSet`, and a whole feature the one whose slice is `NotSet`; `GiveNoteOnSlices` carries them as picked and the receiving side records them as they came. The slices dialog offers the module and the feature of every slice active on the screen and of every slice in `knownSlices`, before the slices in them, so a user can pick the level they mean.

Notes does not record whether the application ran or was only staged. The publication names the application, and its model is what says where a location is.
