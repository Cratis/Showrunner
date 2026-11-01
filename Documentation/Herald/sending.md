---
title: Sending
description: Send an email with IEmails, what the request contains, what is recorded and how a bounce is reported.
---

An application sends through `IEmails`:

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

`Send` renders the email first. If it can not be rendered, an `EmailCannotBeRendered` is thrown and nothing is queued; the message says what is wrong without repeating any value. Otherwise `Send` appends the email to the event log against `About` and returns the `EmailId` every later fact carries. It does not wait for the mail server: [delivery](./delivery-and-retries.md) runs in the background.

Call `Send` from where the decision is made: a reactor that reacts to the fact that an invitation was issued, or an Arc command. Because the email is a fact in the event log first, an application that stops after `Send` returned still delivers the email when it starts.

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
