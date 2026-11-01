---
title: Herald
description: Herald sends the email an application needs, from templates and images in files, over SMTP, and records every email as a fact.
---

Herald is the email part of Showrunner. An application asks Herald to send an email of a kind, such as an invitation, with the values it is about. Herald fills the template for that kind and language, makes the HTML safe for email clients, adds a plain-text alternative and the images, sends it, and records whether it was sent.

Herald is a library, not a service. It runs in the process of the application that hosts it, the way [Usher](../Usher/index.md) and [Marquee](../Marquee/index.md) do, and it keeps its state as events in the application's own Chronicle event store.

- [Add Herald to an application](./add-herald-to-an-application.md): the package, the options and the folders.
- [Templates](./templates.md): one file per email kind and language, the values an email accepts, and the few constructs a template can use.
- [Images](./images.md): which images are embedded and how a template refers to them.
- [Sending](./sending.md): `IEmails`, what the application gives and what it gets back.
- [Delivery and retries](./delivery-and-retries.md): SMTP, the development folder, what happens when the server fails and why the password never appears anywhere.
- [Event reference](./event-reference.md): the facts Herald records and the packages that carry them.

## What Herald promises

- **Nothing is sent with a hole in it.** A value the email does not declare, a required value that is missing, a placeholder the template can not fill, an image that is not there: each stops the email before it is queued, and the error names the problem.
- **A person's words stay words.** Every value is HTML-encoded, so a message someone wrote can not become markup. Templates can not switch the encoding off.
- **A failing server loses nothing.** The email waits in the event log and is retried with growing delays until it is sent or the attempts run out.
- **Secrets stay where they came from.** The mail server's password comes from configuration. It is not a command, an event, a read model, a log line or an exception message.
