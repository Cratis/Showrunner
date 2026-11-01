---
title: Delivery and retries
description: How queued emails are delivered over SMTP or into a folder, what happens when the server fails, and how Herald keeps secrets out of events and logs.
---

Herald delivers in the background. A worker looks, every `PollingInterval`, for emails that were queued and are not settled, reads the email from the event log and sends it through the transport.

## SMTP

The SMTP transport uses MailKit, one connection per email. `Smtp.Security` chooses how the connection is secured:

- `StartTls` connects in the clear and requires an upgrade (port 587).
- `Tls` is TLS from the first byte (port 465).
- `None` is unsecured, for a relay on the same machine or network.

If `Smtp.UserName` is set, Herald signs in with it and `Smtp.Password`. The server's certificate is validated; Herald has no switch to turn that off.

## Development

Set `Transport = EmailTransportKind.Folder` and `PickupFolder` to write each email as an `.eml` file instead of sending it. Open the files in any mail client, or point a mail catcher at the folder. Nothing leaves the machine, and rendering, images and the facts all run as in production.

```csharp
herald.Transport = EmailTransportKind.Folder;
herald.PickupFolder = Path.Combine(builder.Environment.ContentRootPath, "outbox");
```

To send through a provider's API, register your own `IEmailTransport` before calling `AddHerald`; Herald uses it in place of SMTP and the folder.

## When the server fails

An email is settled when `EmailSent` or `EmailFailed` is recorded. Until then it is queued, whatever happens to the application.

| What happened | What Herald does |
| --- | --- |
| The server could not be reached, refused the sign-in, or timed out | Records `EmailAttemptFailed` and tries again after `InitialDelay`, doubling the wait after each failure up to `MaxDelay`. |
| The server refused the message or the recipient for good (a 550-554 reply) | Records `EmailFailed` at once. Sending it again would fail the same way. |
| The last allowed attempt failed | Records `EmailFailed` with the number of attempts. |
| The template or an image is gone when the email is due | Records `EmailFailed` with zero attempts. |

The sign-in failing is retried, not failed for good, because it is a configuration problem that can be fixed while the email waits.

Which emails are waiting is answered from the event log, not from a read model, so a read model that is behind can not make Herald send an email twice. Herald also keeps a read model, `QueuedEmail`, that lists the emails with no outcome yet; use it to show a count of waiting emails.

## Secrets

The mail server's password is read from `SmtpOptions.Password` only to sign in. It is not part of any command, event or read model, `SmtpOptions.ToString()` leaves it out, and the exceptions Herald throws and the log messages it writes carry no credential, no recipient and no value of the email. The reason recorded in `EmailAttemptFailed` and `EmailFailed` is a sentence Herald wrote, with the password and the recipient's address removed from it if they appear. A failure Herald did not write the words for is recorded by its kind only, for example `Delivery failed with SocketException.`

The values of an email can hold names and links, so `EmailQueued` marks them, the address and the name as personal data. A link that works as a credential, such as an invitation token, is stored in the event for as long as the event exists; pass the compliance `Subject` in the request so erasing it erases the link.
