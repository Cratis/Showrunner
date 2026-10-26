// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using Cratis.Arc;
using Cratis.Arc.Identity;
using Cratis.Monads;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Verifies the RS256 tokens AuthProxy authenticates its on-behalf calls with: the key by <c>kid</c>, the issuer, the
/// audience, the lifetime, the purpose, the method, target and body the token is bound to, and that it is used once.
/// </summary>
/// <remarks>
/// Every check fails closed: a missing claim, an unknown key or a token Usher cannot read is a rejection, never a
/// token taken on trust. The token itself is never logged or kept; only its identifier is remembered, until it expires.
/// </remarks>
/// <param name="arc">The <see cref="ArcOptions"/>, whose <see cref="ArcOptions.OnBehalfOf"/> pins AuthProxy's issuer, audience and keys.</param>
/// <param name="options">The <see cref="UsherOptions"/>.</param>
/// <param name="replays">The <see cref="OnBehalfTokenReplays"/>.</param>
/// <param name="time">The clock.</param>
public class OnBehalfTokens(IOptionsMonitor<ArcOptions> arc, IOptionsMonitor<UsherOptions> options, OnBehalfTokenReplays replays, TimeProvider time)
{
    /// <summary>
    /// The longest lifetime Usher accepts a token with. AuthProxy issues them for at most a minute.
    /// </summary>
    public static readonly TimeSpan MaximumLifetime = TimeSpan.FromMinutes(5);

    const string BearerPrefix = "Bearer ";

    /// <summary>
    /// Verifies the token of a call AuthProxy makes to the application.
    /// </summary>
    /// <param name="authorization">The value of the <c>Authorization</c> header.</param>
    /// <param name="call">The <see cref="OnBehalfCall"/> the token must be bound to.</param>
    /// <returns>The <see cref="OnBehalfAssertion"/> the token makes, or why it was rejected.</returns>
    public Task<Result<OnBehalfAssertion, OnBehalfTokenRejection>> Verify(string? authorization, OnBehalfCall call)
    {
        if (authorization?.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase) != true || authorization.Length == BearerPrefix.Length)
        {
            return Task.FromResult<Result<OnBehalfAssertion, OnBehalfTokenRejection>>(OnBehalfTokenRejection.Missing);
        }

        return Verify(authorization[BearerPrefix.Length..].Trim(), call.Purpose, claims =>
        {
            if (!string.Equals(OnBehalfAssertionClaims.Text(claims, "htm"), call.Method, StringComparison.OrdinalIgnoreCase))
            {
                return OnBehalfTokenRejection.WrongMethod;
            }

            if (!IsBoundTo(OnBehalfAssertionClaims.Text(claims, "htu"), call, options.CurrentValue.OnBehalf.BaseUrl))
            {
                return OnBehalfTokenRejection.WrongTarget;
            }

            return OnBehalfAssertionClaims.Text(claims, "body_hash") != Digest(call.Body.Span) ? OnBehalfTokenRejection.WrongBody : null;
        });
    }

    static string Digest(ReadOnlySpan<byte> value) => Base64UrlEncoder.Encode(SHA256.HashData(value));

    static TokenValidationParameters ParametersFor(OnBehalfOfOptions settings) => new()
    {
        ValidIssuer = settings.Issuer,
        ValidAudience = settings.Audience,
        IssuerSigningKeys = OnBehalfSigningKeys.From(settings.SigningKeys),
        ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
        TryAllIssuerSigningKeys = false,
        RequireSignedTokens = true,
        RequireExpirationTime = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = settings.ClockSkew,
    };

    static OnBehalfTokenRejection RejectionFor(Exception? exception) => exception switch
    {
        SecurityTokenSignatureKeyNotFoundException => OnBehalfTokenRejection.UnknownKey,
        SecurityTokenInvalidSignatureException or SecurityTokenInvalidAlgorithmException => OnBehalfTokenRejection.InvalidSignature,
        SecurityTokenInvalidIssuerException => OnBehalfTokenRejection.WrongIssuer,
        SecurityTokenInvalidAudienceException => OnBehalfTokenRejection.WrongAudience,
        SecurityTokenNoExpirationException or SecurityTokenInvalidLifetimeException => OnBehalfTokenRejection.Expired,
        _ => OnBehalfTokenRejection.Malformed
    };

    static bool IsBoundTo(string? target, OnBehalfCall call, Uri? baseUrl)
    {
        if (!Uri.TryCreate(target, UriKind.Absolute, out var bound))
        {
            return false;
        }

        if (baseUrl is null)
        {
            return string.Equals(bound.AbsolutePath, $"{call.PathBase}{call.Path}", StringComparison.Ordinal);
        }

        var expected = new Uri($"{baseUrl.ToString().TrimEnd('/')}{call.Path}");
        return string.Equals(bound.GetLeftPart(UriPartial.Authority), expected.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase)
            && string.Equals(bound.AbsolutePath, expected.AbsolutePath, StringComparison.Ordinal);
    }

    OnBehalfTokenRejection? LifetimeRejection(DateTime? notBefore, DateTime? expires, TimeSpan clockSkew)
    {
        // The lifetime is checked against Usher's own clock, so the whole decision runs on one notion of now, and a
        // token that would live longer than AuthProxy ever issues one for is not taken at its word.
        var now = time.GetUtcNow().UtcDateTime;
        if (expires is not { } validTo || notBefore is not { } validFrom || validTo - validFrom > MaximumLifetime || now > validTo.Add(clockSkew))
        {
            return OnBehalfTokenRejection.Expired;
        }

        return now.Add(clockSkew) < validFrom ? OnBehalfTokenRejection.NotYetValid : null;
    }

    async Task<Result<OnBehalfAssertion, OnBehalfTokenRejection>> Verify(
        string encoded,
        string purpose,
        Func<JsonElement, OnBehalfTokenRejection?> binding)
    {
        var settings = arc.CurrentValue.OnBehalfOf;

        // The lifetime validator runs inside validation; the box carries why it refused out of it.
        var lifetime = new StrongBox<OnBehalfTokenRejection?>();
        var parameters = ParametersFor(settings);
        parameters.LifetimeValidator = (notBefore, expires, _, _) =>
        {
            lifetime.Value = LifetimeRejection(notBefore, expires, settings.ClockSkew);
            return lifetime.Value is null;
        };

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(encoded, parameters);
        if (!validation.IsValid || validation.SecurityToken is not JsonWebToken token)
        {
            return lifetime.Value ?? RejectionFor(validation.Exception);
        }

        using var payload = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(token.EncodedPayload));
        var claims = payload.RootElement;
        if (OnBehalfAssertionClaims.Text(claims, "purpose") != purpose)
        {
            return OnBehalfTokenRejection.WrongPurpose;
        }

        if (binding(claims) is { } unbound)
        {
            return unbound;
        }

        if (OnBehalfAssertionClaims.From(claims) is not { } assertion || OnBehalfAssertionClaims.Text(claims, "jti") is not { Length: > 0 } tokenId)
        {
            return OnBehalfTokenRejection.MissingClaims;
        }

        // The identifier is remembered only for a token that passed every other check, so a forged or misdirected
        // token can never use up the identifier of a real one.
        return replays.TryUse(tokenId, new DateTimeOffset(token.ValidTo, TimeSpan.Zero).Add(settings.ClockSkew))
            ? assertion
            : OnBehalfTokenRejection.Replayed;
    }
}
