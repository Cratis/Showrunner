// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Renders the one-time setup page from the embedded template. Every message is constant: it never repeats what a person
/// typed or what a server answered.
/// </summary>
public static class DirectPage
{
    static readonly string _template = ReadTemplate();

    static string CodeField =>
        "<label for=\"code\">Enrollment code</label><textarea id=\"code\" name=\"code\" required maxlength=\"" + DirectConnection.MaximumCodeLength + "\" spellcheck=\"false\" autocomplete=\"off\"></textarea>";

    static string EndpointField =>
        "<label for=\"endpoint\">Endpoint (optional)</label><input id=\"endpoint\" name=\"endpoint\" type=\"text\" spellcheck=\"false\" autocomplete=\"off\" />";

    /// <summary>
    /// Renders the page.
    /// </summary>
    /// <param name="action">The path the form posts to.</param>
    /// <param name="token">The forgery token the form carries.</param>
    /// <param name="stage">How far the link has come, which decides what the form asks for.</param>
    /// <param name="attempt">What the last attempt came to, or <see langword="null"/> for a fresh page.</param>
    /// <param name="asksForEndpoint">Whether the deployment pins a host, so a person may type an endpoint.</param>
    /// <returns>The HTML.</returns>
    public static string Render(string action, string token, DirectLinkStage stage, DirectAttemptResult? attempt, bool asksForEndpoint)
    {
        var done = attempt == DirectAttemptResult.Connected;
        var needsCode = stage == DirectLinkStage.NotConfigured && !done;
        return _template
            .Replace("__CRATIS_DIRECT_NOTICE__", Notice(attempt, stage), StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_ACTION__", WebUtility.HtmlEncode(action), StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_TOKEN__", WebUtility.HtmlEncode(token), StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_FORM_ATTRIBUTES__", done ? "hidden" : string.Empty, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_CODE_FIELD__", needsCode ? CodeField : string.Empty, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_ENDPOINT_FIELD__", needsCode && asksForEndpoint ? EndpointField : string.Empty, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_SUBMIT__", needsCode ? "Connect" : "Try again", StringComparison.Ordinal);
    }

    static string Notice(DirectAttemptResult? attempt, DirectLinkStage stage) => attempt switch
    {
        DirectAttemptResult.Connected => "<p class=\"notice success\">The product is connected to Chronicle. This page is now closed.</p>",
        DirectAttemptResult.NoCode => Error("Paste the enrollment code."),
        DirectAttemptResult.CodeRefused => Error("The enrollment code was not accepted."),
        DirectAttemptResult.EndpointRefused => Error("The endpoint is not allowed."),
        DirectAttemptResult.Unreachable => Error("Chronicle could not be reached. Try again."),
        DirectAttemptResult.ConnectionFailed => Error("The code was accepted, but connecting to Chronicle failed. Try again; the code is not needed again."),
        DirectAttemptResult.TooManyAttempts => Error("Too many attempts. Wait a while and try again."),
        _ when stage == DirectLinkStage.Redeemed => "<p class=\"notice info\">The code was accepted, but connecting to Chronicle has not succeeded yet. Try again; the code is not needed again.</p>",
        _ => "<p>Paste the enrollment code Chronicle gave you to connect this product to it.</p>"
    };

    static string Error(string message) => $"<p class=\"notice error\">{message}</p>";

    static string ReadTemplate()
    {
        using var stream = typeof(DirectPage).Assembly.GetManifestResourceStream("direct.html")
            ?? throw new InvalidOperationException("The embedded setup page is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
