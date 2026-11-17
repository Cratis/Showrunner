// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Renders the one-time setup page from the embedded template. Every message is constant: it never repeats what a person
/// typed or what a server answered.
/// </summary>
public static class DirectPage
{
    /// <summary>
    /// The one script on the page: when the form is sent it disables the button and says the product is connecting, so a
    /// person sees that something happens while Chronicle answers.
    /// </summary>
    public const string Script =
        "document.addEventListener('submit',function(e){var b=e.target.querySelector('button');if(b){b.disabled=true;b.textContent='Connecting...';}" +
        "var w=document.getElementById('working');if(w){w.hidden=false;}});";

    static readonly string _template = ReadTemplate();

    /// <summary>
    /// Gets the content security policy of the page. It allows only its own styles, the one <see cref="Script"/> by hash, and
    /// posting the form to itself.
    /// </summary>
    public static string ContentSecurityPolicy { get; } =
        $"default-src 'none'; style-src 'unsafe-inline'; script-src 'sha256-{Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(Script)))}'; " +
        "form-action 'self'; frame-ancestors 'none'; base-uri 'none'";

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
        return Fill(
            Notice(attempt, stage),
            action,
            token,
            done,
            needsCode ? CodeField : string.Empty,
            needsCode && asksForEndpoint ? EndpointField : string.Empty,
            needsCode ? "Connect" : "Try again");
    }

    /// <summary>
    /// Renders a page that carries the form again after a post whose form had expired.
    /// </summary>
    /// <param name="action">The path the form posts to.</param>
    /// <param name="token">A fresh forgery token for the form.</param>
    /// <param name="stage">How far the link has come, which decides what the form asks for.</param>
    /// <param name="asksForEndpoint">Whether the deployment pins a host, so a person may type an endpoint.</param>
    /// <returns>The HTML.</returns>
    public static string RenderExpired(string action, string token, DirectLinkStage stage, bool asksForEndpoint)
    {
        var needsCode = stage == DirectLinkStage.NotConfigured;
        return Fill(
            Error("This form had expired, so nothing was done. Paste the enrollment code again and select Connect."),
            action,
            token,
            false,
            needsCode ? CodeField : string.Empty,
            needsCode && asksForEndpoint ? EndpointField : string.Empty,
            needsCode ? "Connect" : "Try again");
    }

    /// <summary>
    /// Renders a page that only says why the request was not answered, with no form.
    /// </summary>
    /// <param name="message">The constant message, as HTML.</param>
    /// <returns>The HTML.</returns>
    public static string RenderRefusal(string message) =>
        Fill(Error(message), string.Empty, string.Empty, true, string.Empty, string.Empty, string.Empty);

    static string Fill(string notice, string action, string token, bool hideForm, string codeField, string endpointField, string submit) =>
        _template
            .Replace("__CRATIS_DIRECT_NOTICE__", notice, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_ACTION__", WebUtility.HtmlEncode(action), StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_TOKEN__", WebUtility.HtmlEncode(token), StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_FORM_ATTRIBUTES__", hideForm ? "hidden" : string.Empty, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_CODE_FIELD__", codeField, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_ENDPOINT_FIELD__", endpointField, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_SUBMIT__", submit, StringComparison.Ordinal)
            .Replace("__CRATIS_DIRECT_SCRIPT__", Script, StringComparison.Ordinal);

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
        _ => "<p class=\"info-text\">Paste the enrollment code you were given in Cratis Direct, then select Connect.</p>"
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
