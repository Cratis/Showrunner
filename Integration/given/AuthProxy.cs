// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// Calls an application exactly as AuthProxy's on-behalf sessions do (Cratis/AuthProxy#217): the grant check and the
/// session notifications, each with the headers AuthProxy sets and a token built like AuthProxy's
/// <c>AttestationSigner</c> and <c>OnBehalfAttestations</c> build it, bound to the method, the target and the exact body.
/// </summary>
public sealed class AuthProxy : IDisposable
{
    /// <summary>
    /// The issuer AuthProxy writes to its tokens.
    /// </summary>
    public const string Issuer = "https://auth.example.com";

    /// <summary>
    /// The audience AuthProxy writes to its tokens.
    /// </summary>
    public const string Audience = "ledger";

    /// <summary>
    /// The name of the application's service in AuthProxy's <c>Services</c>, written to the <c>svc</c> of a forward.
    /// </summary>
    public const string Service = "ledger";

    /// <summary>
    /// The identifier of AuthProxy's signing key.
    /// </summary>
    public const string KeyId = "on-behalf-2026-10";

    readonly RSA _key = RSA.Create(2048);

    /// <summary>
    /// Gets the public half of AuthProxy's signing key, as the application pins it.
    /// </summary>
    public string PublicKeyPem => _key.ExportSubjectPublicKeyInfoPem();

    /// <summary>
    /// Encodes a client principal the way AuthProxy forwards it in a header.
    /// </summary>
    /// <param name="principal">The client principal.</param>
    /// <returns>The base64 header value.</returns>
    public static string Encode(object principal) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(principal, JsonSerializerOptions.Web)));

    /// <summary>
    /// Builds the grant check for a session.
    /// </summary>
    /// <param name="session">The <see cref="OnBehalfSession"/>.</param>
    /// <param name="baseUrl">The address of the application.</param>
    /// <returns>The <see cref="AuthProxyCall"/>.</returns>
    public AuthProxyCall GrantCheck(OnBehalfSession session, Uri baseUrl)
    {
        var target = new Uri(QueryHelpers.AddQueryString($"{baseUrl.ToString().TrimEnd('/')}/.cratis/on-behalf/grants", new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["subject"] = session.Subject,
            ["issue"] = session.Issue,
            ["actor"] = session.Actor.UserId,
            ["session"] = session.SessionId,
        }));
        return new(HttpMethod.Get, target, null, SignCall("on-behalf-grant-check", session, HttpMethod.Get, target, []), session.Tenant, Encode(session.Actor));
    }

    /// <summary>
    /// Builds the notification that a session started.
    /// </summary>
    /// <param name="session">The <see cref="OnBehalfSession"/>.</param>
    /// <param name="baseUrl">The address of the application.</param>
    /// <param name="grantValidUntil">The end of the grant the grant check answered.</param>
    /// <returns>The <see cref="AuthProxyCall"/>.</returns>
    public AuthProxyCall Started(OnBehalfSession session, Uri baseUrl, DateTimeOffset grantValidUntil) =>
        Notification(session, new Uri(baseUrl, "/.cratis/on-behalf/sessions/started"), "on-behalf-session-started", new
        {
            sessionId = session.SessionId,
            tenant = session.Tenant,
            issue = session.Issue,
            subject = session.Subject,
            actor = session.Actor.Notification,
            startedAt = session.StartedAt,
            expiresAt = session.StartedAt.AddHours(1),
            grantValidUntil,
        });

    /// <summary>
    /// Builds the notification that a session ended.
    /// </summary>
    /// <param name="session">The <see cref="OnBehalfSession"/>.</param>
    /// <param name="baseUrl">The address of the application.</param>
    /// <param name="reason">Why the session ended.</param>
    /// <returns>The <see cref="AuthProxyCall"/>.</returns>
    public AuthProxyCall Ended(OnBehalfSession session, Uri baseUrl, string reason) =>
        Notification(session, new Uri(baseUrl, "/.cratis/on-behalf/sessions/ended"), "on-behalf-session-ended", new
        {
            sessionId = session.SessionId,
            tenant = session.Tenant,
            issue = session.Issue,
            subject = session.Subject,
            actor = session.Actor.Notification,
            startedAt = session.StartedAt,
            endedAt = DateTimeOffset.UtcNow,
            reason,
        });

    /// <summary>
    /// Builds the assertion AuthProxy forwards beside a request of a session, bound to the service, the method and the
    /// path of the request and to the exact identity headers.
    /// </summary>
    /// <param name="session">The <see cref="OnBehalfSession"/>.</param>
    /// <param name="request">The request it is forwarded with.</param>
    /// <param name="principalHeader">The exact <c>x-ms-client-principal</c> value.</param>
    /// <param name="actorHeader">The exact <c>x-cratis-on-behalf-actor</c> value.</param>
    /// <returns>The signed assertion.</returns>
    public string Forward(OnBehalfSession session, HttpRequestMessage request, string principalHeader, string actorHeader) =>
        Sign("on-behalf-forward", session, new()
        {
            ["htm"] = request.Method.Method,
            ["htu"] = request.RequestUri!.IsAbsoluteUri ? request.RequestUri.AbsolutePath : request.RequestUri.OriginalString.Split('?')[0],
            ["svc"] = Service,
            ["principal_hash"] = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(principalHeader))),
            ["actor_hash"] = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(actorHeader))),
        });

    /// <inheritdoc/>
    public void Dispose() => _key.Dispose();

    AuthProxyCall Notification(OnBehalfSession session, Uri target, string purpose, object notification)
    {
        var body = JsonSerializer.Serialize(notification, JsonSerializerOptions.Web);
        return new(HttpMethod.Post, target, body, SignCall(purpose, session, HttpMethod.Post, target, Encoding.UTF8.GetBytes(body)), session.Tenant, Encode(session.Actor));
    }

    string SignCall(string purpose, OnBehalfSession session, HttpMethod method, Uri target, byte[] body) =>
        Sign(purpose, session, new()
        {
            ["htm"] = method.Method,
            ["htu"] = target.GetLeftPart(UriPartial.Path),
            ["body_hash"] = Base64UrlEncoder.Encode(SHA256.HashData(body)),
        });

    string Sign(string purpose, OnBehalfSession session, Dictionary<string, object> binding)
    {
        var claims = new Dictionary<string, object>(binding, StringComparer.Ordinal)
        {
            ["purpose"] = purpose,
            ["sub"] = session.Subject,
            ["act"] = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["sub"] = session.Actor.UserId,
                ["idp"] = session.Actor.IdentityProvider,
            },
            ["tenant"] = session.Tenant,
            ["issue"] = session.Issue,
            ["sid"] = session.SessionId,
            [JwtRegisteredClaimNames.Jti] = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)),
        };

        var issuedAt = DateTimeOffset.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = issuedAt.AddMinutes(1).UtcDateTime,
            Claims = claims,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(_key.ExportParameters(true)) { KeyId = KeyId }, SecurityAlgorithms.RsaSha256),
        });
    }
}
