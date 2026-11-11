// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Arc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Authenticates callback assertions and binds notifications to their verified session.
/// </summary>
/// <param name="arc">The Arc options enabling on-behalf sessions.</param>
/// <param name="tokens">The verifier of callback tokens.</param>
/// <param name="logger">The callback logger.</param>
internal sealed class OnBehalfCallVerification(IOptionsMonitor<ArcOptions> arc, OnBehalfTokens tokens, ILogger<OnBehalfSessionCalls> logger)
{
    static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Reads and authenticates a bounded notification before binding its body to the assertion.
    /// </summary>
    /// <param name="http">The callback context.</param>
    /// <param name="purpose">The expected callback purpose.</param>
    /// <returns>The verified notification or the refusal response.</returns>
    internal async Task<(OnBehalfAssertion? Assertion, OnBehalfSessionNotification? Notification, IResult? Failure)> Notification(HttpContext http, string purpose)
    {
        http.Response.Headers.CacheControl = "no-store";
        if (await ReadBody(http.Request) is not { } body)
        {
            return (null, null, Results.StatusCode(StatusCodes.Status413PayloadTooLarge));
        }

        var (assertion, failure) = await Verify(http, purpose, body);
        if (assertion is null)
        {
            return (null, null, failure);
        }

        var notification = Deserialize(body);
        if (notification?.Actor is null || !notification.IsAbout(assertion))
        {
            logger.NotificationNotAboutTheSession(purpose);
            return (null, null, Results.BadRequest());
        }

        return (assertion, notification, null);
    }

    /// <summary>
    /// Verifies the callback token and its tenant header.
    /// </summary>
    /// <param name="http">The callback context.</param>
    /// <param name="purpose">The expected callback purpose.</param>
    /// <param name="body">The exact body authenticated by the token.</param>
    /// <returns>The assertion or the refusal response.</returns>
    internal async Task<(OnBehalfAssertion? Assertion, IResult? Failure)> Verify(HttpContext http, string purpose, ReadOnlyMemory<byte> body)
    {
        http.Response.Headers.CacheControl = "no-store";
        if (arc.CurrentValue.OnBehalfOf?.Enabled != true)
        {
            return (null, Results.NotFound());
        }

        var call = new OnBehalfCall(purpose, http.Request.Method, http.Request.PathBase, http.Request.Path, body);
        var verified = await tokens.Verify(http.Request.Headers.Authorization, call);
        if (verified.TryGetError(out var rejection) || !verified.TryGetResult(out var assertion))
        {
            logger.TokenRejected(purpose, rejection);
            return (null, Results.Unauthorized());
        }

        var tenantHeader = http.Request.Headers[OnBehalfPaths.TenantHeader].ToString();
        if (tenantHeader.Length > 0 && !string.Equals(tenantHeader, assertion.Tenant.Value, StringComparison.Ordinal))
        {
            logger.TenantHeaderDisagrees(purpose);
            return (null, Results.BadRequest());
        }

        return (assertion, null);
    }

    static OnBehalfSessionNotification? Deserialize(byte[] body)
    {
        try
        {
            return JsonSerializer.Deserialize<OnBehalfSessionNotification>(body, _serializerOptions);
        }
        catch (JsonException)
        {
            // An unreadable notification is a bad request; the token already proved it came from AuthProxy.
            return null;
        }
    }

    static async Task<byte[]?> ReadBody(HttpRequest request)
    {
        if (request.ContentLength > OnBehalfSessionCalls.MaximumBodyLength)
        {
            return null;
        }

        await using var body = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = await request.Body.ReadAsync(buffer, request.HttpContext.RequestAborted)) > 0)
        {
            await body.WriteAsync(buffer.AsMemory(0, read), request.HttpContext.RequestAborted);
            if (body.Length > OnBehalfSessionCalls.MaximumBodyLength)
            {
                return null;
            }
        }

        return body.ToArray();
    }
}
