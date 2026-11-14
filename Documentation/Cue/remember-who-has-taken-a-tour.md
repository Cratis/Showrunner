---
title: Remember who has taken a tour
description: The Cue backend records the first time a person starts a guided tour as an event, exposes what they started as a query, and validates tour names against the application's list.
---

A button that points at a tour should stop pointing once the person has taken it, on every device they use. `Cratis.Showrunner.Cue` records that and nothing more: it does not play tours and holds no script.

## What is recorded

`StartTour` is the command the application calls when a person starts a tour. It appends `TourStarted` under the person's own event source, once. Starting the same tour again appends nothing, so how often a person watches a tour changes nothing about whether they have seen it. Someone who is not signed in has nothing to remember, and the command is refused.

`StartedTour.ByMe` is the query that gives the tours the signed-in person has started. The frontend reads it through `useCueTours` and `useTourNudge`, and answers `hasStarted` from the person's own history and from what they started on this device a moment ago, since a query follows the event log and may not have caught up yet.

## Configure the tours

`AddCue` takes `CueOptions`:

```csharp
builder.AddCue(cue => cue.Tours = ["order-desk", "month-end"]);
```

`Tours` lists the tour names the application accepts. A name that is not on the list, or is empty, is refused with a validation message. Left empty, any name is accepted, which leaves the application nothing to guard against a client that invents tours.

## Who is signed in

Cue tells people apart by the `sub` claim of the current principal, or the name identifier when there is none. An application that identifies people by something else registers its own `ISignedInViewer` before `AddCue`:

```csharp
builder.Services.AddTransient<ISignedInViewer, MyViewer>();
```

`ISignedInViewer` says whether someone is signed in and gives their `ViewerSubject`. Whatever it returns identifies a person in the application's own terms, so it should be stable.

## Personal data

The history is recorded under an event source derived from the person's subject by a one-way hash, so the subject itself never becomes an identifier that is stored in the clear. The event carries the subject marked as personal data, with the person as the compliance subject, so an erasure request for that person finds what Cue recorded about them and erases it. See the [event reference](./event-reference.md).

## Read the history from the backend

An application that wants to know on the server which tours a person started, such as to carry it on the identity it publishes, asks `IViewerTours`:

```csharp
var started = await tours.Of(subject);
var hasStarted = started.Has("order-desk");
```
