// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using Cratis.Arc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// Issues on-behalf tokens exactly as AuthProxy's AttestationSigner and OnBehalfAttestations do
/// (Cratis/AuthProxy#217): RS256 with the key identifier in the header, iss, aud, iat, nbf, exp, a random jti, and the
/// purpose, sub, act, tenant, issue and sid claims, plus htm, htu and body_hash for a call.
/// </summary>
public sealed class AuthProxyTokens : IDisposable
{
    public const string Issuer = "https://auth.example.com";
    public const string Audience = "portal";
    public const string KeyId = "on-behalf-2026-10";

    readonly RSA _key = RSA.Create(2048);

    public string PublicKeyPem => _key.ExportSubjectPublicKeyInfoPem();

    public static IOptionsMonitor<ArcOptions> ArcOptions(AuthProxyTokens authProxy)
    {
        var options = Substitute.For<IOptionsMonitor<ArcOptions>>();
        options.CurrentValue.Returns(new ArcOptions
        {
            TrustForwardedIdentityHeaders = true,
            OnBehalfOf = new()
            {
                Enabled = true,
                Issuer = Issuer,
                Audience = Audience,
                ClockSkew = TimeSpan.FromSeconds(5),
                SigningKeys = [new() { KeyId = KeyId, PublicKeyPem = authProxy.PublicKeyPem }]
            }
        });
        return options;
    }

    public static string Digest(byte[] value) => Base64UrlEncoder.Encode(SHA256.HashData(value));

    public string ForCall(
        string purpose,
        HttpMethod method,
        Uri target,
        byte[] body,
        OnBehalfSessionFacts session,
        DateTimeOffset issuedAt,
        TimeSpan? lifetime = null,
        string issuer = Issuer,
        string audience = Audience,
        RSA? key = null,
        string keyId = KeyId) =>
        Sign(session, issuedAt, lifetime ?? TimeSpan.FromMinutes(1), issuer, audience, key ?? _key, keyId, new(StringComparer.Ordinal)
        {
            ["purpose"] = purpose,
            ["htm"] = method.Method,
            ["htu"] = target.GetLeftPart(UriPartial.Path),
            ["body_hash"] = Digest(body),
        });

    public string ForForward(OnBehalfSessionFacts session, string principalHeader, string actorHeader, DateTimeOffset issuedAt) =>
        Sign(session, issuedAt, TimeSpan.FromMinutes(1), Issuer, Audience, _key, KeyId, new(StringComparer.Ordinal)
        {
            ["purpose"] = "on-behalf-forward",
            ["htm"] = "GET",
            ["htu"] = "/api/issues",
            ["svc"] = "portal",
            ["principal_hash"] = Digest(System.Text.Encoding.ASCII.GetBytes(principalHeader)),
            ["actor_hash"] = Digest(System.Text.Encoding.ASCII.GetBytes(actorHeader)),
        });

    public void Dispose() => _key.Dispose();

    string Sign(OnBehalfSessionFacts session, DateTimeOffset issuedAt, TimeSpan lifetime, string issuer, string audience, RSA key, string keyId, Dictionary<string, object> binding)
    {
        var claims = new Dictionary<string, object>(binding, StringComparer.Ordinal)
        {
            ["sub"] = session.Subject,
            ["act"] = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["sub"] = session.Actor,
                ["idp"] = session.ActorIdentityProvider,
            },
            ["tenant"] = session.Tenant,
            ["issue"] = session.Issue,
            ["sid"] = session.SessionId,
            [JwtRegisteredClaimNames.Jti] = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = issuedAt.Add(lifetime).UtcDateTime,
            Claims = claims,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(key.ExportParameters(true)) { KeyId = keyId }, SecurityAlgorithms.RsaSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}

public record OnBehalfSessionFacts(string SessionId, string Tenant, string Issue, string Subject, string Actor, string ActorIdentityProvider);
#endif
