// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An HTTP handler that records requests and answers from a function, so no specification touches the network.
/// </summary>
/// <param name="respond">Answers a request; it may throw to simulate a transport failure.</param>
public sealed class StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    /// <summary>
    /// Gets the requests, with their bodies read.
    /// </summary>
    public List<RecordedRequest> Requests { get; } = [];

    /// <summary>
    /// Creates a handler that answers every request with a status.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The handler.</returns>
    public static StubHttpHandler Answering(HttpStatusCode status) => new(_ => new HttpResponseMessage(status));

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(new RecordedRequest(
            request.Method,
            request.RequestUri!,
            request.Headers.Authorization?.ToString(),
            request.Headers.UserAgent.ToString(),
            request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken)));
        return respond(request);
    }
}
#endif
