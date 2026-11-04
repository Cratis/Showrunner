---
title: Sending
description: Send an email with IEmails, what the request contains, what is recorded and how a bounce is reported.
---

After [adding Herald to the application](./add-herald-to-an-application.md) and preparing the [invitation template](./templates.md) and any [images](./images.md), inject `IEmails` where the application decides to invite someone. The reactor below is an application excerpt, not a complete host or template setup.

An invitation link can act as a credential. Herald records `Values` in the queued event, so that link remains there for as long as the event exists. Choose the compliance subject before sending real tokens. When `Subject` is unset, as in this excerpt, the event source is the erasure association; set `Subject` explicitly when the personal data belongs to another subject. The [delivery and retention guidance](./delivery-and-retries.md) explains the longer-lived record and erasure association.

The reactor maps the invitation's recipient, language and declared values into the request:

```csharp
public class InviteReactor(IEmails emails) : IReactor
{
    public async Task Invited(InvitationIssued @event, EventContext context) =>
        await emails.Send(new EmailRequest(
            Kind: "invitation",
            About: context.EventSourceId,
            To: @event.Email,
            Values: new Dictionary<string, string>
            {
                ["invitation.link"] = @event.Link,
                ["invitation.expiresAt"] = @event.ExpiresAt.ToString("O"),
                ["inviter.name"] = @event.InviterName,
                ["inviter.message"] = @event.Message
            })
        {
            ToName = @event.Name,
            Language = @event.Language
        });
}
```

| Part of the request | Meaning |
| --- | --- |
| `Kind` | The email kind, which is the name of its [template](./templates.md). |
| `About` | What the email is about, as an event source id, such as the invitation. The facts about the email are recorded against it. |
| `To`, `ToName` | The recipient. The address must be an email address; a `Name <address>` form is accepted. |
| `Values` | The values to fill the email with, by the names the manifest declares. Dates are ISO 8601. |
| `Language` | Optional. The language to write the email in. |
| `Subject` | Optional. The compliance subject the personal data of the email belongs to, for erasure. |

`Send` renders first. If rendering fails, it throws `EmailCannotBeRendered` and queues nothing; the message identifies the problem without repeating a value. Correct the named template, image or value before trying again. A rejected event-log append also throws rather than returning an identifier.

After a successful append against `About`, `Send` returns the `EmailId` carried by later facts. Keep the two together to follow this email in the application's event log. The return value means queued, not accepted by the mail server; [delivery](./delivery-and-retries.md) happens in the background.

Call `Send` from the decision point: the reactor for an issued invitation, or an Arc command. Once a request is queued, use its recorded outcome to follow delivery rather than assuming a restart or another call means it reached the recipient. A worker can retry a waiting email, stop at a permanent failure or exhaust its attempts; the events below tell those outcomes apart.

## What is recorded

The event log of the application gets, against `About`:

1. `EmailQueued` when `Send` accepted the email. It holds the recipient and the values, marked as personal data, so erasing the subject erases them.
2. `EmailAttemptFailed` for each delivery the server did not take, with when the next one happens.
3. `EmailSent`, or `EmailFailed` when the email will not be sent. These two are in [`Cratis.Showrunner.Herald.Contracts`](./event-reference.md); the application can project and react to them.

## Bounces

An SMTP server accepting an email does not know whether it reached the mailbox. When the application learns that it did not, from a bounce message or the provider, it records it:

```csharp
await emails.Bounced(about, emailId, "invitation", "Mailbox full");
```

That appends `EmailBounced` against `about`. Leave the recipient's address out of the reason.
