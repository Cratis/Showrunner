---
title: Acknowledging
description: How a person acknowledges a dialog once, why a banner can not be acknowledged, and why looking is never recorded.
---

An announcement shown with `Display.DialogOnce` is shown to a person until they acknowledge it. The acknowledgement is a fact: `AnnouncementAcknowledged(AnnouncementId, UserSubject)`, recorded in the **tenant's** namespace under the announcement's id, with the person as compliance subject. That is the one place where a tenant's own namespace holds anything about announcements; the announcements themselves are product-wide, see [Receiving](./receiving.md).

## The command

`AcknowledgeAnnouncement(AnnouncementId)` is in the `Acknowledging/AcknowledgeAnnouncement` slice and requires a signed-in person.

| The announcement is | The command |
| --- | --- |
| a `DialogOnce` for this tenant, not acknowledged by the person | records `AnnouncementAcknowledged` |
| a `DialogOnce` the person already acknowledged | succeeds and records nothing |
| a `DialogEverySession` | succeeds and records nothing: it returns at the next session whatever the person did, and looking is not a fact |
| a `Banner` | refused: a banner can not be dismissed |
| not held by the product, or not for the person's tenant | refused |

Acknowledging twice appends nothing: the command takes the tenant's acknowledgements as a read-model parameter, and the product runs with read protection `On`, so two concurrent acknowledgements by the same person record one fact.

## Acknowledged on every device

The acknowledgement belongs to the person, not the browser. `UserSubject` is the `sub` claim of the signed-in principal (or the name identifier), the same subject on every device, so a dialog acknowledged on one device is gone on the next.

## Personal data

`UserSubject` is marked `[PII]`, and the event's compliance subject is a one-way hash of it, `PersonSubjects.For(subject)`, not the subject itself. To erase a person, compute that subject and delete its key in every tenant namespace they signed in to.
