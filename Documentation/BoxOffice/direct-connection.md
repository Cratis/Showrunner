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

```

`AddBoxOfficeProduct` registers `ChronicleDirectEnrollment`, the production `IDirectEnrollment`, which redeems the code through `IChronicleClient.Federation`. It is only built when the page or the configured code needs it, so a product that leaves `Direct.Enabled` off never touches Chronicle's federation API. Register your own `IDirectEnrollment` before `AddBoxOfficeProduct` to replace it.

| Option | Default | Meaning |
| --- | --- | --- |
| `Enabled` | `false` | The page, and redemption from configuration, exist. |
| `Path` | `/.cratis/showrunner/direct` | The page's path. |
| `Administrators` | empty | The subjects, as AuthProxy presents them, who may use the page. Empty means nobody; the first person to arrive never becomes the administrator. |
| `ExpectedEndpointHost` | empty | The one host the product may link to. A manually typed endpoint is accepted only when this is set and the endpoint is HTTPS on this host. |
| `AllowReenrollment` | `false` | The page may open again, but only when the link is also unrecoverable. |
| `EnrollmentCode` | empty | A code to redeem at startup without the page. Supply it from a secret store or the environment. |
| `MaximumAttempts` / `AttemptWindow` | 5 / 15 minutes | How many attempts the page and the configured code may make together. |

Redeeming needs Chronicle prerequisites; see [What Chronicle needs](#what-chronicle-needs).

## Three factors

1. The request comes through AuthProxy with an authenticated principal (`x-ms-client-principal-id`). Without one the page answers `401`.
2. The principal is in `Administrators`. Otherwise it answers `403`.
3. The enrollment code, which carries the endpoint, the fingerprint of the certificate authority and a secret.

The form post is protected against cross-site forgery with a token bound to the person and the time the page was shown, valid for one hour, and a post marked cross-site by the browser is refused. The token needs no cookie, so a browser that withholds cookies after the sign-in redirect can still post the form. A post whose form has expired gets the form again with a message, and nothing is redeemed.

Every answer is a page that says what happened, including a refusal. While the code is being redeemed the page disables *Connect* and says it is connecting; its one script is allowed by hash in the page's content security policy. A refused post is logged with its reason. Messages are constant: they never repeat the code, the endpoint or what a server answered, and neither is logged. There is no way to point the product at an arbitrary endpoint.

## What happens to the link

| Stage | The page |
| --- | --- |
| Not configured | Open; asks for the code. |
| Redeemed | The credentials are stored, but the first exchange has not succeeded. The page stays open, shows the reason and offers *Try again*, which reuses the stored credentials and needs no second code. |
| Closed | The first exchange succeeded. The page answers not found for good. |

The state is recorded as events (`DirectLinkRedeemed`, `DirectLinkVerified`, `DirectLinkReopened`) in the product's own event store, so a closed page stays closed across restarts. With `AllowReenrollment` on, a closed or redeemed link whose exchange reports `Unrecoverable` opens the page again.

## Without the page

Set `Direct.EnrollmentCode` (from the environment or a secret) and the product redeems it once at startup through the same state machine. Starting again with the same code is harmless: once the link is closed nothing is redeemed.

## What Chronicle needs

The code is redeemed on the Chronicle kernel the product is connected to, not by the product itself: the kernel keeps the certificate it is issued, renews it and dials the authority. This page needs Chronicle 19.164.0 or later, which has `IChronicleClient.Federation` and the connection state in the enrollment status, and a kernel with hosted federation.

**The application's Chronicle principal must be allowed to redeem.** Redeeming a code and reading the enrollment status are decided by the kernel capability `chronicle.federation.enroll`. Either:

- the principal the product connects to Chronicle with is an administrator, or
- its client id is listed in the kernel setting `Federation:Enrollment:Applications`, which grants it this one capability and nothing else (it cannot create or revoke enrollments). A change takes effect when the kernel restarts:

```json
{
  "Chronicle": {
    "Federation": {
      "Enrollment": {
        "Applications": [ "my-product" ]
      }
    }
  }
}
```

Without it the kernel refuses, and the page can only say that the code was not accepted. Look at the product's log, which then names the cause as a refused redemption.

**Providing the code to the kernel instead.** The kernel can redeem a code without the product: put the code in a file and set `Federation:Enrollment:CodePath` to it. A kernel that is not enrolled redeems it at startup and deletes the file. When the product then asks, the kernel is already enrolled and the product reports the link as redeemed without using a code.

**Dial-only mode.** To make the product's kernel the dialing side of the link, so that it opens no listener and needs no open port, leave `Federation:Enabled` off and set `Federation:Dial`:

```json
{
  "Chronicle": {
    "Federation": {
      "Dial": {
        "Enabled": true,
        "EventStore": "customer",
        "Namespace": "Default",
        "LogicalSource": "hosted"
      }
    }
  }
}
```

`Dial:EventStore` and `Dial:Namespace` name the local event store whose inbox receives the authority's events. The kernel trusts the authority root that came with the code and keeps the connection up through renewals. The other kernel settings of enrollment (`Federation:Enrollment:AllowedHosts`, `AllowRestrictedAddresses`) are in Chronicle's federation documentation.

### What the adapter can and cannot tell

- **One answer for every failure.** The kernel answers every failure to redeem the same way, whether the code is wrong, expired or used, or the authority is unreachable, wrong or refusing. The adapter reports `Unreachable` only when the Chronicle kernel itself cannot be reached, and `Refused` for any answer from it.
- **A typed endpoint cannot replace the code's.** The Chronicle API takes only the code, so the kernel connects to the endpoint the code carries. A typed endpoint is accepted only if it names the same authority as the code, and a code whose endpoint cannot be read is refused when a host is pinned. The pinned host is checked before the kernel connects.
- **An enrolled kernel is a redeemed link.** If the kernel already holds a certificate that has not expired, the adapter reports `Redeemed` with the kernel's endpoint and leaves the code unspent. An expired enrollment is redeemed again with the new code.
- **"First exchange succeeded" is the kernel's connection state.** `Verify` reads `FederationEnrollmentStatus` and reports `Healthy` only when `Connected` is true and `LastExchange` has a time, which the kernel sets only after a successful exchange. A kernel that is not enrolled or whose certificate has expired is `Unrecoverable`, because only a new enrollment restores it. A status that cannot be read, or an enrolled kernel that is not connected, is `Unreachable`, so the page stays open and a retry needs no new code. A kernel that has never exchanged is given `ConnectionPatience` (ten seconds by default) to make its first exchange after enrollment, because it dials only then; one that exchanged before and has lost the connection is reported at once. The kernel's `LastError` is scrubbed of secrets by the kernel and is written to the log only, truncated; the page shows its own fixed message and never the kernel's text.
- **The product's event store is not derived from the code.** The code and the status carry the endpoint and the identity, not an event store name. `GuestListOptions.OperatorStores` and the kernel's `Dial:EventStore` stay settings.

## The port

The product registers an `IDirectEnrollment`. It is the only place that talks to Chronicle, and `ChronicleDirectEnrollment` is the production implementation:

```csharp
public interface IDirectEnrollment
{
    Task<DirectRedemptionResult> Redeem(DirectEnrollmentRequest request, CancellationToken cancellationToken);
    Task<DirectLinkHealth> Verify(CancellationToken cancellationToken);
}
```

`Redeem` redeems the code through `IChronicleClient.Federation.Redeem`, refuses an endpoint that is not on `ExpectedHost`, and returns the endpoint of the enrollment. `Verify` reads `IChronicleClient.Federation.GetStatus` and reports `Healthy` (connected, with a last exchange), `Unreachable` (a retry may fix it) or `Unrecoverable`. Neither logs the code.
