// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Reads what an AuthProxy token says about an on-behalf session from its claims.
/// </summary>
public static class OnBehalfAssertionClaims
{
    /// <summary>
    /// Reads the session from the claims of a token whose signature was already verified.
    /// </summary>
    /// <param name="claims">The JSON payload of the token.</param>
    /// <returns>The <see cref="OnBehalfAssertion"/>, or <see langword="null"/> when a claim the session needs is missing.</returns>
    public static OnBehalfAssertion? From(JsonElement claims)
    {
        if (claims.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var subject = Text(claims, "sub");
        var tenant = Text(claims, "tenant");
        var issue = Text(claims, "issue");
        var sessionId = Text(claims, "sid");
        var actor = claims.TryGetProperty("act", out var act) && act.ValueKind == JsonValueKind.Object ? act : default;
        var actorSubject = actor.ValueKind == JsonValueKind.Object ? Text(actor, "sub") : null;
        if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(tenant) || string.IsNullOrEmpty(issue) ||
            string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(actorSubject))
        {
            return null;
        }

        return new(subject, actorSubject, Text(actor, "idp") ?? string.Empty, tenant, issue, sessionId);
    }

    /// <summary>
    /// Reads the session from a forward assertion Arc has already verified for the request it came with.
    /// </summary>
    /// <remarks>
    /// The signature is not checked again: call it only for the exact header value Arc authenticated the request with,
    /// which Arc does only after verifying its signature, issuer, audience, lifetime, purpose, service, method, path,
    /// identity headers, tenant and token identifier.
    /// </remarks>
    /// <param name="assertion">The compact JWS of the <c>x-cratis-on-behalf-assertion</c> header.</param>
    /// <returns>The <see cref="OnBehalfAssertion"/>, or <see langword="null"/> when it can not be read or is not a forward assertion.</returns>
    public static OnBehalfAssertion? FromVerifiedForward(string assertion)
    {
        var parts = assertion.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            using var payload = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(parts[1]));
            return payload.RootElement.ValueKind == JsonValueKind.Object && Text(payload.RootElement, "purpose") == OnBehalfPaths.ForwardPurpose
                ? From(payload.RootElement)
                : null;
        }
        catch (Exception exception) when (exception is JsonException or FormatException or ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Reads a string claim.
    /// </summary>
    /// <param name="claims">The JSON object to read from.</param>
    /// <param name="name">The claim.</param>
    /// <returns>The value, or <see langword="null"/> when the claim is missing or not a string.</returns>
    public static string? Text(JsonElement claims, string name) =>
        claims.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}
