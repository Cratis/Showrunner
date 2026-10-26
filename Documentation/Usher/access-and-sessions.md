---
title: Access and support sessions
description: How a reporter grants support access to one issue, how support signs in as the reporter through AuthProxy's on-behalf sessions, how Usher answers AuthProxy's grant check and session notifications, and what the reporter sees of it.
---

A reporter can let support act as them on one issue for a fixed time. Support then signs in as the reporter through [AuthProxy's on-behalf sessions](https://github.com/Cratis/AuthProxy/blob/main/Documentation/configuration/on-behalf-sessions.md), reproduces the problem with the reporter's own data and permissions, and everything support does is recorded as done by support on the reporter's behalf. The reporter sees every session and what was done in it, and can revoke the grant at any time.

Usher is the application side of AuthProxy's on-behalf sessions: it decides whether a grant is live, says who support acts as, and records when sessions start and end. AuthProxy never lets support act as anybody without Usher's answer.

## Grants

The reporter grants access with `GrantAccess(IssueId, AccessDuration)`, or by choosing to when reporting. A grant:

- is for **one issue**; support can never use it on the reporter's other issues;
- ends at a **fixed time**, `OneHour`, `OneDay` or `SevenDays` from when it was given, recorded as `ValidUntil` on `ReporterGrantedAccess`;
- always **shares who the reporter is** first (`ReporterSharedIdentity`), because support can only act as someone it knows;
- ends when the reporter **revokes** it (`RevokeAccess`, recorded as `ReporterRevokedAccess`), when it **lapses** (Usher's background work records `AccessGrantLapsed` at most `PollingInterval` after `ValidUntil` passes), or when the reporter **withdraws** the issue (`WithdrawIssue`, recorded as `ReporterWithdrewIssue`). A grant is never live past `ValidUntil`, lapse recorded or not. Withdrawing is final, so a withdrawn issue can not be granted again, nothing is left to revoke, and nothing lapses later.

Withdrawing is published to the desk as `IssueWithdrawn` alone, not also as `AccessRevoked`: a desk treats a withdrawn issue as closed to support, access included, so a second public fact would tell it the same thing twice.

## Map the endpoints

AuthProxy calls three endpoints on the application. They are plain HTTP endpoints, not Arc commands, because AuthProxy signs the exact bytes of each call. Map them with `MapUsher`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddCratis();
builder.AddUsher(usher =>
{
    usher.ApplicationName = "Ledger";
    builder.Configuration.GetSection("Usher").Bind(usher);
});

var app = builder.Build();
app.UseCratis();
app.MapUsher();
app.Run();
```

| Endpoint | AuthProxy calls it | Usher answers |
| --- | --- | --- |
| `GET /.cratis/on-behalf/grants` | once before a session starts, and again at every `OnBehalf:RecheckInterval` | `{ "live": true, "validUntil", "principal" }` while the grant lets support act as the reporter in that session, otherwise `{ "live": false }` |
| `POST /.cratis/on-behalf/sessions/started` | before it issues a session | `204` after recording `SupportSignedInAsReporter`; `403` with `{ "error": "<reason>" }` when the grant does not let the session start, so AuthProxy never issues it |
| `POST /.cratis/on-behalf/sessions/ended` | when it notices a session ended | `204`, after recording `SupportSessionEnded` the first time |

The endpoints allow anonymous callers: nobody is signed in on these calls, and AuthProxy authenticates with its signed token instead. Until on-behalf sessions are configured they answer `404`.

`AddUsher` also puts a check first in the request pipeline, through an `IStartupFilter`, for every request that carries `x-cratis-on-behalf-actor` or `x-cratis-on-behalf-assertion`:

- It asks Arc's authentication for the request, and refuses it with `401` unless Arc accepted it as an on-behalf forward, with the actor beside the represented user. Arc verifies AuthProxy's assertion itself; see [What Arc verifies](#what-arc-verifies-on-a-forwarded-request). A forward Arc does not accept never reaches the application, not even anonymously.
- It refuses the request with `401` unless the assertion names the represented user and the actor Arc accepted, and the tenant in `x-cratis-tenant-id`.
- It decides from the issue's events whether the session is live, and keeps the decision with the request for Usher's commands and queries.

This check only stops a forged **on-behalf session**: a caller can not claim to be support acting as a reporter without AuthProxy's signature. It does not authenticate anything else. Arc believes the forwarded identity headers of a host that trusts them, so a request with a plain `x-ms-client-principal` and no on-behalf headers is believed as that user. The backend must therefore be reachable **only through AuthProxy**, and the ingress in front of AuthProxy must strip every identity header (`x-ms-client-principal`, `x-cratis-on-behalf-actor`, `x-cratis-on-behalf-assertion`, `x-cratis-tenant-id`) that arrives from outside.

## Configure AuthProxy, Arc and Usher together

AuthProxy signs two kinds of token with its `OnBehalf:Attestation` keys: the assertion beside every forwarded request, which **Arc** verifies, and the token on each call to Usher's endpoints, which **Usher** verifies. Both use Arc's `Cratis:Arc:OnBehalfOf` section, so the issuer, the audience and the keys are configured once, and on-behalf sessions are on exactly when it is enabled:

```json
{
  "Cratis": {
    "Arc": {
      "TrustForwardedIdentityHeaders": true,
      "OnBehalfOf": {
        "Enabled": true,
        "Issuer": "https://auth.example.com",
        "Audience": "ledger",
        "Service": "ledger",
        "ClockSkew": "00:00:30",
        "SigningKeys": [
          { "KeyId": "on-behalf-2026-10", "PublicKeyPem": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----" }
        ]
      }
    }
  },
  "Usher": {
    "OnBehalf": {
      "BaseUrl": "https://ledger.internal",
      "AgentIdentityProviders": ["usher-agents"]
    }
  }
}
```

The matching AuthProxy configuration:

```json
{
  "Cratis": {
    "AuthProxy": {
      "Services": {
        "ledger": { "Backend": { "BaseUrl": "https://ledger.internal" } }
      },
      "OnBehalf": {
        "Service": "ledger",
        "Attestation": {
          "Issuer": "https://auth.example.com",
          "Audience": "ledger",
          "ActiveKeyId": "on-behalf-2026-10",
          "SigningKeys": [
            { "KeyId": "on-behalf-2026-10", "PrivateKeyPem": "<from a secret store>" }
          ]
        }
      }
    }
  }
}
```

| Setting | Must match in AuthProxy | Meaning |
| --- | --- | --- |
| `Cratis:Arc:TrustForwardedIdentityHeaders` | | Must be `true`: the represented user and the actor arrive in AuthProxy's forwarded identity headers. |
| `Cratis:Arc:OnBehalfOf:Enabled` | | Turns on-behalf sessions on. Off, Arc refuses every forward that carries an actor, and Usher's endpoints answer `404`. |
| `Cratis:Arc:OnBehalfOf:Issuer` | `OnBehalf:Attestation:Issuer` | The issuer every token must carry. |
| `Cratis:Arc:OnBehalfOf:Audience` | `OnBehalf:Attestation:Audience` | The audience every token must carry. |
| `Cratis:Arc:OnBehalfOf:SigningKeys` | `OnBehalf:Attestation:SigningKeys` | The **public** half of each key AuthProxy signs with, by `KeyId`. RSA, at least 2048 bits. A private key is refused. |
| `Cratis:Arc:OnBehalfOf:Service` | the name of the service in `Services`, the one `OnBehalf:Service` names | The service a forward assertion must be issued for (`svc`), so an assertion for another service behind the same AuthProxy is refused. Empty leaves `svc` unchecked. Set it. |
| `Cratis:Arc:OnBehalfOf:ClockSkew` | | How far the clocks may disagree. 30 seconds by default. |
| `Usher:OnBehalf:BaseUrl` | `Services:<service>:Backend:BaseUrl` | When set, a call's token must be bound to this exact host as well as the path. When not set, only the path is compared, because behind a proxy the application cannot tell the address AuthProxy used. Set it. |
| `Usher:OnBehalf:AgentIdentityProviders` | | Identity providers whose users are agents, not people; see [What the reporter sees](#what-the-reporter-sees). |

With `OnBehalfOf` enabled, the host refuses to start without `TrustForwardedIdentityHeaders`, without an issuer or an audience, or without usable keys. Supply the keys from configuration; they are not secrets, but they decide who may act as your users, so keep them where configuration is reviewed.

To rotate a key, add the new public key to `SigningKeys` first, then move AuthProxy's `ActiveKeyId` to it, and remove the old key once every token it signed has expired.

The host must also be reachable only through AuthProxy.

## What Arc verifies on a forwarded request

During a session AuthProxy forwards every request with `x-cratis-on-behalf-assertion`. Arc accepts the request as the represented user with the actor beside them only when the assertion:

1. is RS256, signed by a key in `SigningKeys` selected by `kid`;
2. carries the configured `iss` and `aud`, is within `nbf` and `exp`, and lives no longer than five minutes;
3. has the `purpose` `on-behalf-forward`, and the `svc` of `Service` when it is set;
4. is bound to this request: `htm` is its method and `htu` its path as the backend received it (path base and path, without host or query);
5. is bound to the exact `x-ms-client-principal` and `x-cratis-on-behalf-actor` values (`principal_hash`, `actor_hash`), and its `sub` and `act.sub` are those users;
6. names the request's tenant, and its `jti` has not been seen before.

Usher then reads which issue (`issue`) and which session (`sid`) the request belongs to from the assertion Arc accepted, without verifying it a second time.

During a session AuthProxy also asks the application's `/.cratis/me` as the represented user, with an assertion bound to `GET` and the identity endpoint's path. Arc verifies it like any forward, so an application's identity details provider sees the actor as the principal's `ClaimsIdentity.Actor` (`IsActedOnBehalfOf()`) and can tell support access from the reporter's own sign-in. Usher records nothing about sign-ins on that call.

## How Usher verifies a call

Each call to Usher's endpoints carries `Authorization: Bearer <token>`, an RS256 token signed by AuthProxy. Usher:

1. selects the public key by `kid` and requires RS256;
2. validates `iss`, `aud`, `nbf` and `exp` against its own clock, and refuses a token that lives longer than five minutes;
3. requires the `purpose` of the endpoint: `on-behalf-grant-check`, `on-behalf-session-started` or `on-behalf-session-ended`;
4. compares `htm` and `htu` with the request, and `body_hash` with the SHA-256 of the exact body read before deserializing; the grant check hashes the empty body;
5. takes who acts (`act`), for whom (`sub`), in which tenant (`tenant`), on which issue (`issue`) and in which session (`sid`) from the token, never from the query or the body. A notification whose body names another session, tenant, issue, subject or actor than its token is refused with `400`, and so is an `x-cratis-tenant-id` other than the token's tenant;
6. accepts each `jti` once, until the token expires.

A token that fails any check is answered with `401`. Usher logs why, never the token.

## The grant check

The answer is live only when, at that moment and from the issue's own events:

- the user asked about reported the issue, and the actor is somebody else;
- a grant is in effect: not revoked, not lapsed, the issue not withdrawn, and `ValidUntil` has not passed;
- the reporter shared who they are;
- the session asked about has not ended. An ended session never resumes, so a copied session cookie is refused everywhere;
- the session asked about has started, or has never been asked about before. AuthProxy checks the grant once before it announces a new session, with the identifier the session will have, so the first check for a session that has not started is live and records `SupportSessionPermitted`. Every later check for it is not live until Usher has acknowledged the start. A session Usher never recorded as started is therefore never kept alive by asking again, on any instance: AuthProxy's requirement that the grant check refuse every `sid` it never acknowledged as started holds from the second check on.

An unknown tenant or issue is answered `{ "live": false }`, the same as an issue without a grant, so the check reveals nothing about which issues exist.

A live answer carries `validUntil`, the end of the grant as an ISO 8601 date and time that always states its offset (`+00:00`; AuthProxy refuses a value without `Z` or an offset), and `principal`, the reporter as they shared who they are:

```json
{
  "live": true,
  "validUntil": "2026-10-26T13:00:00+00:00",
  "principal": {
    "identityProvider": "entra",
    "userId": "<the reporter's subject, exactly as asked>",
    "userDetails": "Ada Lovelace",
    "userRoles": [],
    "claims": [{ "typ": "name", "val": "Ada Lovelace" }, { "typ": "email", "val": "ada@example.com" }]
  }
}
```

AuthProxy forwards this principal as the user on every request of the session, so it must carry the roles and claims the application authorizes on. Usher only knows what the reporter shared; an application that authorizes on roles registers its own `IRepresentedPrincipals` to add them. The answer is never live when the principal names anybody other than the reporter.

## Recording sessions

`SupportSessionPermitted(SessionId)` is recorded once per session, by the first grant check that answers live for a session that has not started. It is what makes every later check for a session that never started not live.

`SupportSignedInAsReporter(SessionId, Author, Actor)` is recorded when AuthProxy announces a session, only after the same checks as the grant check, and guarded by a concurrency scope on the issue's access events: a grant revoked between the check and the append refuses the start. The session identifier is AuthProxy's opaque `sessionId`. `Author` is the actor's `userDetails` and whether they are a `Person` or an `Agent`: an actor from one of `Usher:OnBehalf:AgentIdentityProviders` is an agent, everybody else a person. `Actor` is who signed in at their identity provider, the `act` subject and `idp` of AuthProxy's assertion.

`SupportSessionEnded(SessionId)` is recorded once per session, whatever reports the end:

- AuthProxy's notification, for any of its reasons, from any number of replicas. The reason is not recorded; `identity-flow-started`, sent when the browser enters an invitation, registration or account-link flow, ends the session like any other;
- the reporter revoking the grant, the grant lapsing, or the reporter withdrawing the issue. Usher ends every session open on the grant itself, without waiting for AuthProxy to notice, so the reporter sees the session end with the grant. AuthProxy stops the session at its next grant check, at most `OnBehalf:RecheckInterval` later, and its notification is then acknowledged without recording anything again.

The lapse of an earlier grant after a new one was given ends nothing. An end reported for a session that never started is acknowledged and not recorded.

The start and the end are recorded with the actor first and the reporter second as the identity that caused them, like everything done in the session.

## What support does in a session

During a session, AuthProxy forwards each request as the reporter with the actor beside them, and Arc makes the principal the reporter with the actor as its [`ClaimsIdentity.Actor`](https://github.com/Cratis/Arc/blob/main/Documentation/backend/csharp/asp-net-core/acting-on-behalf-of.md). Usher's commands therefore run exactly as for the reporter: they check that the signed-in user reported the issue, and the represented user did. Arc gives Chronicle the actor first and the reporter as `OnBehalfOf`, so every event records both.

The grant is for one issue, and so is what support can change:

- support may act only in a **live** session. For every forwarded request, the forward check folds the issue's events, the same decision as the grant check, except that the session must have started: the grant is in effect and not past its end, the issue is not withdrawn, and this session started and has not ended. A revocation, a lapse, a withdrawal or the end of the session counts from the moment it is recorded, not at AuthProxy's next grant check;
- every command on an existing issue (`AddDetails`, `AnswerQuestion`, `ShareIdentity`, `ReopenIssue`, `WithdrawIssue`, `RevokeAccess`) runs for support only on the issue the session is for, as AuthProxy's verified assertion states it. Any other of the reporter's issues is answered exactly like an issue that does not exist;
- support can not report in the reporter's name: `ReportIssue` and `ReportError` are refused for support signed in as the reporter, whatever they share or grant. Only the reporter themselves can grant access, so `GrantAccess` is refused too;
- the queries are narrowed to the session: `MyIssues` lists only the issue the session is for, and `ForIssue` answers nothing for any other issue. Once the session is no longer live both answer nothing, and an observation of `ForIssue` that outlives a revocation, a lapse or the end of the session stops showing the issue.

## What the reporter sees

The `IssueConversation` query shows the reporter, in `Access.Sessions`, every session with who signed in, when it started and when it ended. Each conversation entry has:

| Property | Meaning |
| --- | --- |
| `ActedBy` | The support person or agent who did it while signed in as the reporter. Empty when the reporter, the desk or Usher did it. |
| `Session` | The session it was done in, when it was done on this issue in an open session of that actor. |

Usher attributes an entry from the identity that caused the event: when it acts on behalf of somebody, the actor's user name (Arc takes it from the `userDetails` AuthProxy forwards) is `ActedBy`, and the open session on the issue of the actor with the same subject is `Session`. The session is matched by subject, not by name, because two people can share a display name; the causation names no identity provider, so the subject is all both sides have. The read model holds a one-way hash of the subject (`ActorKey`), not the subject itself. Events record who caused them, not the session, so:

- an action on another of the reporter's issues names who acted but no session on that issue;
- when two open sessions on the same issue have actors with the same subject, no session is credited;
- an action that lands after the session's end was recorded names who acted but no session.

## Security notes

- **Usher decides from events, every time.** Nothing about a grant is cached: each check, and each forwarded request, folds the issue's events, so a revocation counts in Usher from the moment it is recorded.
- **Outside Usher, revocation is bounded by AuthProxy's re-check interval.** AuthProxy honors its last live answer for at most `OnBehalf:RecheckInterval`, per instance, and keeps forwarding the session to the rest of the application until then. Keep it short.
- **Replay is refused per process.** A `jti` is remembered in memory by the instance that accepted it, by Usher for AuthProxy's calls and by Arc for forwarded requests, so the refusal holds only when every call reaches the same instance: run a single replica, or route AuthProxy's calls and forwarded requests to one instance with sticky sessions. A token replayed to another replica within its lifetime of at most five minutes is accepted once more there. It is still bound to its call, its body, its identity headers and its purpose, and every call is idempotent, so a replayed start or end records nothing new, and a replayed grant check for a session that has not started finds its permit and is not live; a replayed forward assertion lets the request through once more on that replica, and Usher's own commands and queries still refuse it once the session is no longer live.
- **Keep the backend reachable only through AuthProxy.** Arc and the forward check refuse unsigned on-behalf forwards, but a host that trusts forwarded identity headers believes a plain `x-ms-client-principal` from anybody who can reach it. Strip identity headers at the ingress.
- **Personal data.** Sessions record the actor's name, and the grant check returns the reporter's name and email. They are the reporter's and support's data, recorded in the tenant's namespace; see [Personal data](./backend.md#personal-data).
