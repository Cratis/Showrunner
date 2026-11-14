// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;
using Cratis.Showrunner.BoxOffice.Product.SignIns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

public class a_direct_page : a_direct_connection
{
    protected const string Administrator = "1001";

    protected DirectConnectionCalls _calls;
    protected DirectForgery _forgery;

    /// <summary>
    /// What the page answered.
    /// </summary>
    /// <param name="Status">The status code.</param>
    /// <param name="Html">The body.</param>
    /// <param name="Nonce">The nonce the cookie carries, empty when none was set.</param>
    /// <param name="Token">The forgery token the form carries, empty when there is no form.</param>
    /// <param name="Headers">The response headers.</param>
    protected record Answer(int Status, string Html, string Nonce, string Token, IHeaderDictionary Headers);

    static string TokenIn(string html)
    {
        const string marker = "name=\"token\" value=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        return start < 0 ? string.Empty : html[(start + marker.Length)..html.IndexOf('"', start + marker.Length)];
    }

    protected static DefaultHttpContext Request(string method, string? principal)
    {
        var http = new DefaultHttpContext { RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider() };
        http.Request.Method = method;
        http.Request.Path = DirectConnectionOptions.DefaultPath;
        http.Response.Body = new MemoryStream();
        if (principal is not null)
        {
            http.Request.Headers[SignInHeaders.PrincipalId] = principal;
        }

        return http;
    }

    protected static DefaultHttpContext Form(string? principal, string? nonce, string? token, string code = "", string? endpoint = null, string? site = null)
    {
        var http = Request(HttpMethods.Post, principal);
        var fields = new List<string> { $"token={Uri.EscapeDataString(token ?? string.Empty)}", $"code={Uri.EscapeDataString(code)}" };
        if (endpoint is not null)
        {
            fields.Add($"endpoint={Uri.EscapeDataString(endpoint)}");
        }

        var body = Encoding.UTF8.GetBytes(string.Join('&', fields));
        http.Request.ContentType = "application/x-www-form-urlencoded";
        http.Request.ContentLength = body.Length;
        http.Request.Body = new MemoryStream(body);
        if (nonce is not null)
        {
            http.Request.Headers.Cookie = $"{DirectForgery.CookieName}={nonce}";
        }

        if (site is not null)
        {
            http.Request.Headers["Sec-Fetch-Site"] = site;
        }

        return http;
    }

    protected async Task<Answer> Send(DefaultHttpContext http)
    {
        var result = await _calls.Receive(http);
        await result.ExecuteAsync(http);
        http.Response.Body.Position = 0;
        var html = await new StreamReader(http.Response.Body).ReadToEndAsync();
        var cookie = http.Response.Headers.SetCookie.ToString();
        var nonce = cookie.StartsWith(DirectForgery.CookieName + "=", StringComparison.Ordinal) ? cookie[(DirectForgery.CookieName.Length + 1)..].Split(';')[0] : string.Empty;
        return new(http.Response.StatusCode, html, nonce, TokenIn(html), http.Response.Headers);
    }

    protected Task<Answer> Open(string principal = Administrator) => Send(Request(HttpMethods.Get, principal));

    protected async Task<Answer> Submit(string code, string? endpoint = null, string principal = Administrator)
    {
        var page = await Open(principal);
        return await Send(Form(principal, page.Nonce, page.Token, code, endpoint));
    }

    void Establish()
    {
        _options.Direct.Administrators = [Administrator];
        _forgery = new();
        _calls = new(_connection, _forgery, Options.Create(_options));
    }
}
#endif
