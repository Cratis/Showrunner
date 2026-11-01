// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;

namespace Cratis.Showrunner.Notes.Transport.given;

/// <summary>
/// An HTTP handler that keeps the requests it saw and answers what a specification says.
/// </summary>
/// <param name="answer">Creates the response to a request.</param>
public class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> answer) : HttpMessageHandler
{
    readonly List<(HttpMethod Method, Uri Uri, string Authorization, string Body)> _requests = [];

    /// <summary>
    /// Gets the requests seen.
    /// </summary>
    public IReadOnlyList<(HttpMethod Method, Uri Uri, string Authorization, string Body)> Requests => _requests;

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        _requests.Add((request.Method, request.RequestUri!, request.Headers.Authorization?.ToString() ?? string.Empty, body));
        return answer(request);
    }

    /// <summary>
    /// Creates a response with a JSON body.
    /// </summary>
    /// <param name="json">The body.</param>
    /// <returns>The response.</returns>
    public static HttpResponseMessage Json(string json) => new(HttpStatusCode.OK) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
}
#endif
