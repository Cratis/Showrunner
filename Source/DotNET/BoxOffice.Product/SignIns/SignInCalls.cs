// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Showrunner.BoxOffice.Product.Activity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns;

/// <summary>
/// Receives AuthProxy's sign-in callback and reports the person as active.
/// </summary>
/// <remarks>
/// The call is AuthProxy's own when it carries AuthProxy's marker header, which AuthProxy strips from every request that
/// comes through it, and the shared secret when one is configured. The person it names must also be the person AuthProxy
/// presents on the call, so a call can only ever report the person it is made for.
/// </remarks>
/// <param name="activity">The <see cref="IReportActivity"/> that tells the operator.</param>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
public class SignInCalls(IReportActivity activity, IOptions<BoxOfficeProductOptions> options, ILogger<SignInCalls> logger)
{
    /// <summary>
    /// The longest body of a callback that is read.
    /// </summary>
    public const int MaximumBodyLength = 16 * 1024;

    static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Receives a sign-in callback.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/> of the call.</param>
    /// <returns>The <see cref="IResult"/> to answer with.</returns>
    public async Task<IResult> Receive(HttpContext http)
    {
        http.Response.Headers.CacheControl = "no-store";

        if (!IsFromAuthProxy(http.Request))
        {
            logger.NotFromAuthProxy();
            return Results.Unauthorized();
        }

        if (await ReadNotification(http.Request) is not { } notification)
        {
            logger.BadCallback("the body is missing, too long or not a sign-in notification");
            return Results.BadRequest();
        }

        var principal = http.Request.Headers[SignInHeaders.PrincipalId].ToString();
        if (!string.Equals(principal, notification.Subject, StringComparison.Ordinal))
        {
            logger.BadCallback("the person the call names is not the person AuthProxy presents");
            return Results.BadRequest();
        }

        var tenant = TenantOf(http.Request, notification);
        if (tenant is null)
        {
            logger.BadCallback("the tenant in the body and the tenant AuthProxy resolved disagree");
            return Results.BadRequest();
        }

        if (tenant == TenantId.NotSet)
        {
            logger.NoTenant();
            return Results.NoContent();
        }

        var person = new Person(notification.IdentityProvider, notification.Subject, notification.Name ?? string.Empty, notification.Email ?? string.Empty);
        await activity.Report(tenant, person, http.RequestAborted);
        return Results.NoContent();
    }

    static TenantId? TenantOf(HttpRequest request, SignInNotification notification)
    {
        var header = request.Headers[SignInHeaders.Tenant].ToString();
        if (header.Length == 0)
        {
            header = request.Headers[SignInHeaders.LegacyTenant].ToString();
        }

        var body = notification.Tenant ?? string.Empty;
        if (header.Length > 0 && body.Length > 0 && !string.Equals(header, body, StringComparison.Ordinal))
        {
            return null;
        }

        return new(body.Length > 0 ? body : header);
    }

    static bool IsKind(string kind) => string.Equals(kind, "SignedIn", StringComparison.Ordinal) || string.Equals(kind, "Returned", StringComparison.Ordinal);

    static async Task<SignInNotification?> ReadNotification(HttpRequest request)
    {
        if (request.ContentLength > MaximumBodyLength)
        {
            return null;
        }

        await using var body = new MemoryStream();
        var buffer = new byte[4096];
        int read;
        while ((read = await request.Body.ReadAsync(buffer, request.HttpContext.RequestAborted)) > 0)
        {
            await body.WriteAsync(buffer.AsMemory(0, read), request.HttpContext.RequestAborted);
            if (body.Length > MaximumBodyLength)
            {
                return null;
            }
        }

        try
        {
            var notification = JsonSerializer.Deserialize<SignInNotification>(body.ToArray(), _serializerOptions);
            return notification is not null &&
                IsKind(notification.Kind) &&
                !string.IsNullOrWhiteSpace(notification.Subject) &&
                !string.IsNullOrWhiteSpace(notification.IdentityProvider)
                    ? notification
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Compares two secrets. Both sides are hashed first, so the comparison takes the same time whatever the lengths are.
    /// </summary>
    /// <param name="expected">The secret that is configured.</param>
    /// <param name="given">The secret the call carries.</param>
    /// <returns><see langword="true"/> when they are the same.</returns>
    static bool Matches(string expected, string given) =>
        CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(expected)), SHA256.HashData(Encoding.UTF8.GetBytes(given)));

    bool IsFromAuthProxy(HttpRequest request)
    {
        if (!IsKind(request.Headers[SignInHeaders.Marker].ToString()))
        {
            return false;
        }

        var secret = options.Value.SignInSecret;
        return secret.Length == 0 || Matches(secret, request.Headers[SignInHeaders.Secret].ToString());
    }
}
