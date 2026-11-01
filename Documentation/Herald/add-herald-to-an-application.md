---
title: Add Herald to an application
description: Reference the Herald package, call AddHerald with the templates folder, the sender and the mail server, and lay out the template and image files.
---

Herald needs the application to be a Chronicle client, because it records its facts in the application's event store. Reference `Cratis.Showrunner.Herald` and call `AddHerald` next to `AddCratis` (or `AddCratisChronicle`). The order does not matter. Herald records in the event store the host registers, without you naming it again; set `EventStore` in the options only to use another one.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddHerald(herald =>
{
    herald.TemplatesFolder = Path.Combine(builder.Environment.ContentRootPath, "Emails");
    herald.ImagesFolder = Path.Combine(builder.Environment.ContentRootPath, "Emails", "images");
    herald.Sender = new() { Address = "noreply@ledger.example.com", Name = "Ledger" };
    herald.Look = new() { BrandColor = "#0b5fff", Footer = "Ledger AS, Storgata 1, Oslo" };
    herald.Smtp = new()
    {
        Host = "smtp.example.com",
        Port = 587,
        Security = SmtpSecurity.StartTls,
        UserName = "ledger",
        Password = builder.Configuration["Herald:SmtpPassword"]!
    };
});
```

Adding Herald registers [`IEmails`](./sending.md), which the application sends with, and a background worker that delivers and retries. Chronicle finds Herald's events and its read model in the `Cratis.Showrunner.Herald` assembly, because Cratis type discovery includes referenced packages whose names start with `Cratis`. The options are validated when the host starts, and a problem is reported with the option to set.

## Options

| Option | Default | Meaning |
| --- | --- | --- |
| `TemplatesFolder` | none, required | The folder with the [templates](./templates.md). It must exist. |
| `ImagesFolder` | none | The folder with the [images](./images.md). Optional when no template has an image. |
| `DefaultLanguage` | `en` | The language used when an email has no template in the language asked for. |
| `Sender.Address` | none, required | The address the emails come from. |
| `Sender.Name` | empty | The name shown beside the address. |
| `Sender.ReplyTo` | empty | Where replies go, when not to the sender. |
| `Look.BrandColor` | `#2563eb` | The color of buttons and links, as `#rrggbb`. Templates use `{{look.brandColor}}`. |
| `Look.Footer` | empty | The footer text shared by every template. Templates use `{{look.footer}}`. |
| `Transport` | `Smtp` | `Smtp`, or `Folder` to write `.eml` files for [development](./delivery-and-retries.md#development). |
| `Smtp.Host`, `Smtp.Port` | none, `587` | The mail server. |
| `Smtp.Security` | `StartTls` | `StartTls` (usually port 587), `Tls` (usually 465) or `None`, for a relay on the same machine or network. |
| `Smtp.UserName`, `Smtp.Password` | empty | How to sign in. Leave both empty for a relay that does not ask. |
| `PickupFolder` | none | The folder the `Folder` transport writes to. |
| `Retry.MaxAttempts` | `6` | How many deliveries are tried before the email fails for good. |
| `Retry.InitialDelay`, `Retry.MaxDelay` | 30 seconds, 1 hour | The wait after the first failure, doubling after each next one up to the longest. |
| `PollingInterval` | 5 seconds | How often Herald looks for queued emails that are due. |
| `Namespace` | the default namespace | The event store namespace the facts are recorded in. |
| `EventStore` | the application's own event store | The event store the facts are recorded in. Herald uses the event store of the application's `AddCratis` or `AddCratisChronicle` setup; set it only to record in another one. |

The password is a secret. Read it from the environment or the application's secret store, as the example does, and never from a file in the repository.

## Run one worker

Herald's worker delivers what is queued. Delivery is at least once: if the server accepts an email and the application stops before the fact is recorded, the email is delivered again at the next start. Run the worker in a single instance of the application to avoid two instances picking up the same email at once.
