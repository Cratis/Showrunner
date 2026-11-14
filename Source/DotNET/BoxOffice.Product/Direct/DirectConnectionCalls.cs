// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Product.SignIns;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Answers the one-time setup page: shows it, and receives the form that redeems the enrollment code.
/// </summary>
/// <remarks>
/// A person gets in only when the request comes through AuthProxy with an authenticated principal and that principal is
/// one of the configured administrators; the enrollment code is the third factor. A closed link answers as if the page
/// never existed.
/// </remarks>
/// <param name="connection">The <see cref="DirectConnection"/>.</param>
/// <param name="forgery">The <see cref="DirectForgery"/>.</param>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
public class DirectConnectionCalls(DirectConnection connection, DirectForgery forgery, IOptions<BoxOfficeProductOptions> options)
{
    /// <summary>
    /// The longest form that is read.
    /// </summary>
    public const int MaximumBodyLength = 16 * 1024;

    /// <summary>
    /// Receives a request for the page.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/> of the request.</param>
    /// <returns>The <see cref="IResult"/> to answer with.</returns>
    public async Task<IResult> Receive(HttpContext http)
    {
        var headers = http.Response.Headers;
        headers.CacheControl = "no-store";
        headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; form-action 'self'; frame-ancestors 'none'; base-uri 'none'";
        headers.XFrameOptions = "DENY";
        headers.XContentTypeOptions = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";

        var person = http.Request.Headers[SignInHeaders.PrincipalId].ToString();
        if (string.IsNullOrWhiteSpace(person))
        {
            return Results.Unauthorized();
        }

        if (!options.Value.Direct.Administrators.Contains(person, StringComparer.Ordinal))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var state = await connection.Current(http.RequestAborted);
        if (state.Stage == DirectLinkStage.Closed)
        {
            return Results.NotFound();
        }

        return HttpMethods.IsPost(http.Request.Method)
            ? await Post(http, person)
            : Page(http, person, state.Stage, null);
    }

    static string Field(IFormCollection form, string name) => form.TryGetValue(name, out var value) ? value.ToString() : string.Empty;

    static bool IsCrossSite(HttpRequest request)
    {
        var site = request.Headers["Sec-Fetch-Site"].ToString();
        return site.Length > 0 && site is not ("same-origin" or "none");
    }

    async Task<IResult> Post(HttpContext http, string person)
    {
        if (IsCrossSite(http.Request) ||
            !http.Request.HasFormContentType ||
            http.Request.ContentLength is null or > MaximumBodyLength)
        {
            return Results.BadRequest();
        }

        var form = await http.Request.ReadFormAsync(http.RequestAborted);
        var nonce = http.Request.Cookies[DirectForgery.CookieName] ?? string.Empty;
        if (!forgery.IsValid(nonce, Field(form, "token"), person))
        {
            return Results.BadRequest();
        }

        var result = await connection.Attempt(Field(form, "code"), Field(form, "endpoint"), http.RequestAborted);
        var stage = (await connection.Current(http.RequestAborted)).Stage;
        return result == DirectAttemptResult.Closed
            ? Results.NotFound()
            : Page(http, person, stage, result);
    }

    IResult Page(HttpContext http, string person, DirectLinkStage stage, DirectAttemptResult? attempt)
    {
        var nonce = DirectForgery.NewNonce();
        http.Response.Cookies.Append(DirectForgery.CookieName, nonce, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        var settings = options.Value.Direct;
        var html = DirectPage.Render(settings.Path, forgery.TokenFor(nonce, person), stage, attempt, settings.ExpectedEndpointHost.Length > 0);
        return Results.Content(html, "text/html; charset=utf-8", statusCode: attempt == DirectAttemptResult.TooManyAttempts ? StatusCodes.Status429TooManyRequests : null);
    }
}
