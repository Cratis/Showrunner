---
title: Event reference
description: The ReleaseNotesSeen event Playbill records when a person has seen a release's notes, the properties it carries and how it is kept as personal data in the event log.
---
Playbill appends one event.

## ReleaseNotesSeen

A person saw the release notes up to a version, and will not be told about those releases again. Only a step forward is a fact: marking a release the person has already seen, or an older one, records nothing.

| Property | Meaning |
| --- | --- |
| `Version` | The newest release the person has seen the notes of. |
| `Viewer` | The person, by the subject the application knows them by. Personal data. |

The event is appended under an event source derived from the person's subject, `playbill-viewer-` followed by a SHA-256 hash of the subject, so the subject itself is not stored in the event source id. The compliance subject is the same value, so erasing the person erases what Playbill holds about them.

## Reading it

`SeenReleaseNotes` is the person's progress: one read model per person, holding the newest version they have seen. It is built from `ReleaseNotesSeen` and nothing else, and it only moves forward whatever order the events are replayed in.

## Changing it

The event has one generation. A change to its shape is a new event type beside it, not a change to this one, so that the history already recorded keeps meaning what it meant.
