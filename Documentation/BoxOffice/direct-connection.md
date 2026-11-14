# The Direct connection page

A product that Box Office manages needs a link to its Chronicle. The Direct connection page is a one-time setup page where an administrator pastes one **enrollment code** and the product links itself. It is **off by default**: while it is off, `/.cratis/showrunner/direct` does not exist and answers not found.

## Turn it on

```csharp
builder.AddBoxOfficeProduct(options =>
{
    options.Direct.Enabled = true;
    options.Direct.Administrators = ["1001"];
    options.Direct.ExpectedEndpointHost = "chronicle.example.com";
});

builder.Services.AddSingleton<IDirectEnrollment, ChronicleDirectEnrollment>();
```

| Option | Default | Meaning |
| --- | --- | --- |
| `Enabled` | `false` | The page, and redemption from configuration, exist. |
| `Path` | `/.cratis/showrunner/direct` | The page's path. |
| `Administrators` | empty | The subjects, as AuthProxy presents them, who may use the page. Empty means nobody; the first person to arrive never becomes the administrator. |
| `ExpectedEndpointHost` | empty | The one host the product may link to. A manually typed endpoint is accepted only when this is set and the endpoint is HTTPS on this host. |
| `AllowReenrollment` | `false` | The page may open again, but only when the link is also unrecoverable. |
| `EnrollmentCode` | empty | A code to redeem at startup without the page. Supply it from a secret store or the environment. |
| `MaximumAttempts` / `AttemptWindow` | 5 / 15 minutes | How many attempts the page and the configured code may make together. |

Enabling the page without registering an `IDirectEnrollment` fails at startup.

## Three factors

1. The request comes through AuthProxy with an authenticated principal (`x-ms-client-principal-id`). Without one the page answers `401`.
2. The principal is in `Administrators`. Otherwise it answers `403`.
3. The enrollment code, which carries the endpoint, the fingerprint of the certificate authority and a secret.

The form post is protected against cross-site forgery with a cookie nonce and a token bound to the person, and a post marked cross-site by the browser is refused. Messages are constant: they never repeat the code, the endpoint or what a server answered, and neither is logged. There is no way to point the product at an arbitrary endpoint.

## What happens to the link

| Stage | The page |
| --- | --- |
| Not configured | Open; asks for the code. |
| Redeemed | The credentials are stored, but the first exchange has not succeeded. The page stays open, shows the reason and offers *Try again*, which reuses the stored credentials and needs no second code. |
| Closed | The first exchange succeeded. The page answers not found for good. |

The state is recorded as events (`DirectLinkRedeemed`, `DirectLinkVerified`, `DirectLinkReopened`) in the product's own event store, so a closed page stays closed across restarts. With `AllowReenrollment` on, a closed or redeemed link whose exchange reports `Unrecoverable` opens the page again.

## Without the page

Set `Direct.EnrollmentCode` (from the environment or a secret) and the product redeems it once at startup through the same state machine. Starting again with the same code is harmless: once the link is closed nothing is redeemed.

## The port

The product registers an `IDirectEnrollment`. It is the only place that talks to Chronicle:

```csharp
public interface IDirectEnrollment
{
    Task<DirectRedemptionResult> Redeem(DirectEnrollmentRequest request, CancellationToken cancellationToken);
    Task<DirectLinkHealth> Verify(CancellationToken cancellationToken);
}
```

`Redeem` redeems the code through Chronicle's federation enrollment, stores the issued certificate, refuses an endpoint that is not on `ExpectedHost`, and returns the endpoint taken from the code. `Verify` makes an exchange with the stored credentials and reports `Healthy`, `Unreachable` (a retry may fix it) or `Unrecoverable`.
