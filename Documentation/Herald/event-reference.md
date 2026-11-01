---
title: Event reference
description: The facts Herald records, the contracts package that carries them, and why they are private to the application's event log.
---

Herald records two kinds of events in the event store of the application that hosts it.

## Contracts

`Cratis.Showrunner.Herald.Contracts` holds the facts a host reacts to and projects. They are recorded against whatever the email was about and carry the `EmailId` the application got from `Send`. None carries the recipient, a value of the email or a credential.

| Event | Recorded when | Properties |
| --- | --- | --- |
| `EmailSent` | The mail server accepted the email. | `EmailId`, `Kind`, `Language`, `Attempts` |
| `EmailFailed` | The email will not be sent: it could not be rendered, the server refused it for good, or the attempts ran out. | `EmailId`, `Kind`, `Language`, `Reason`, `Attempts` |
| `EmailBounced` | The application reported that an accepted email did not arrive. | `EmailId`, `Kind`, `Reason` |

These events are not `[Public]`. Public events are accepted only by the outbox and refused by the event log, and Herald records in the event log so that the application's own projections and reactors can follow them, for example to mark an invitation as sent. Read them in a projection:

```csharp
[FromEvent<EmailSent>]
public record InvitationDelivery(
    [property: Key] EventSourceId InvitationId,
    DateTimeOffset? SentAt);
```

## Herald's own events

`Cratis.Showrunner.Herald` has two more events that carry the delivery:

| Event | Recorded when |
| --- | --- |
| `EmailQueued` | `Send` accepted the email. Holds the kind, the language asked for, the recipient and the values. The recipient and the values are personal data. |
| `EmailAttemptFailed` | A delivery failed and Herald will try again. Holds the attempt number, the reason and when the next attempt is due. |

## Changing an event

The contracts package has a snapshot of every event's schema, and a specification that fails when the schema of a released generation changes. A change to a released event is a new generation of the event, with a migration, never an edit.
