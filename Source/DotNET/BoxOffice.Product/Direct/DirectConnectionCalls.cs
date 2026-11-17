// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Product.SignIns;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Answers the one-time setup page: shows it, and receives the form that redeems the enrollment code.
/// </summary>
/// <remarks>
/// A person gets in only when the request comes through AuthProxy with an authenticated principal and that principal is
/// one of the configured administrators; the enrollment code is the third factor. A closed link answers as if the page
/// never existed. Every answer is a page that says what happened: a browser shows an answer without a body as nothing, or
/// downloads it as an empty file, and the person cannot tell whether anything was done.
/// </remarks>
/// <param name="connection">The <see cref="DirectConnection"/>.</param>
/// <param name="forgery">The <see cref="DirectForgery"/>.</param>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
/// <param name="logger">The <see cref="ILogger{T}"/>.</param>
public class DirectConnectionCalls(
    DirectConnection connection,
    DirectForgery forgery,
    IOptions<BoxOfficeProductOptions> options,
    ILogger<DirectConnectionCalls> logger)
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
        headers.ContentSecurityPolicy = DirectPage.ContentSecurityPolicy;
        headers.XFrameOptions = "DENY";
        headers.XContentTypeOptions = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";

        var person = http.Request.Headers[SignInHeaders.PrincipalId].ToString();
        if (string.IsNullOrWhiteSpace(person))
        {
            return Refusal(StatusCodes.Status401Unauthorized, "Sign in to use this page.");
        }

        if (!options.Value.Direct.Administrators.Contains(person, StringComparer.Ordinal))
        {
            logger.NotAnAdministrator();
            return Refusal(StatusCodes.Status403Forbidden, "You are not one of the people who may connect this product.");
        }

        var state = await connection.Current(http.RequestAborted);
        if (state.Stage == DirectLinkStage.Closed)
        {
            return NotFound();
        }

        return HttpMethods.IsPost(http.Request.Method)
            ? await Post(http, person, state.Stage)
            : Page(person, state.Stage, null);
    }

    static string Field(IFormCollection form, string name) => form.TryGetValue(name, out var value) ? value.ToString() : string.Empty;

    static bool IsCrossSite(HttpRequest request)
    {
        var site = request.Headers["Sec-Fetch-Site"].ToString();
        return site.Length > 0 && site is not ("same-origin" or "none");
    }

    static IResult Refusal(int status, string message) =>
        Results.Content(DirectPage.RenderRefusal(message), "text/html; charset=utf-8", statusCode: status);

    static IResult NotFound() => Refusal(StatusCodes.Status404NotFound, "There is nothing here.");

    async Task<IResult> Post(HttpContext http, string person, DirectLinkStage stage)
    {
        if (IsCrossSite(http.Request))
        {
            logger.PostRefused("it came from another site");
            return Refusal(StatusCodes.Status400BadRequest, "The form was sent from another site, so nothing was done.");
        }

        if (!http.Request.HasFormContentType || http.Request.ContentLength is null or > MaximumBodyLength)
        {
            logger.PostRefused("it was not a form of an acceptable length");
            return Refusal(StatusCodes.Status400BadRequest, "The form could not be read, so nothing was done. Open the page again and paste the code.");
        }

        var form = await http.Request.ReadFormAsync(http.RequestAborted);
        if (!forgery.IsValid(Field(form, "token"), person))
        {
            logger.PostRefused("its form token was missing, expired or made for someone else");
            var settings = options.Value.Direct;
            return Results.Content(
                DirectPage.RenderExpired(settings.Path, forgery.TokenFor(person), stage, settings.ExpectedEndpointHost.Length > 0),
                "text/html; charset=utf-8",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await connection.Attempt(Field(form, "code"), Field(form, "endpoint"), http.RequestAborted);
        var current = (await connection.Current(http.RequestAborted)).Stage;
        return result == DirectAttemptResult.Closed
            ? NotFound()
            : Page(person, current, result);
    }

    IResult Page(string person, DirectLinkStage stage, DirectAttemptResult? attempt)
    {
        var settings = options.Value.Direct;
        var html = DirectPage.Render(settings.Path, forgery.TokenFor(person), stage, attempt, settings.ExpectedEndpointHost.Length > 0);
        return Results.Content(html, "text/html; charset=utf-8", statusCode: attempt == DirectAttemptResult.TooManyAttempts ? StatusCodes.Status429TooManyRequests : null);
    }
}
