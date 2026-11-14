---
title: Event reference
description: The event Cue records when a person starts a guided tour, what it carries and how it is kept as personal data.
---

Cue appends one event.

## TourStarted

A person started a guided tour. Only the first start of a tour is a fact: starting it again records nothing.

| Property | Meaning |
| --- | --- |
| `Tour` | The name of the tour, as the application knows it. Renaming a tour makes every person look as if they never started it. |
| `Viewer` | The person, by the subject the application knows them by. Personal data. |

The event is appended under an event source derived from the person's subject, `cue-viewer-` followed by a SHA-256 hash of the subject, so the subject itself is not stored in the event source id. The compliance subject is the same value, so erasing the person erases what Cue holds about them.

## Reading it

`StartedTours` is the person's history: one read model per person, with the tours in the order they first started them. It is built from `TourStarted` and nothing else, and counts a tour once however often it was started.

## Changing it

The event has one generation. A change to its shape is a new event type beside it, not a change to this one, so that the history already recorded keeps meaning what it meant.
