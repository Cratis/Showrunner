// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// Serves a <see cref="FakeDirectServer"/> to an <see cref="HttpClient"/>, so the real client talks to it without a network.
/// </summary>
/// <param name="server">The server.</param>
public sealed class FakeDirectHandler(FakeDirectServer server) : HttpMessageHandler
{
    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (server.Unreachable)
        {
            throw new HttpRequestException("connection refused");
        }

        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        var epoch = request.Headers.TryGetValues("X-Direct-Watchdog-Epoch", out var values) ? values.FirstOrDefault() : null;
        var (status, content) = server.Handle(request.Method.Method, request.RequestUri!.AbsolutePath, request.RequestUri.Query, request.Headers.Authorization?.Parameter, epoch, body);
        if (server.DropResponses && request.RequestUri.AbsolutePath.EndsWith("/events", StringComparison.Ordinal))
        {
            throw new HttpRequestException("the connection was lost before the answer arrived");
        }

        return new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(content, Encoding.UTF8, "application/json") };
    }
}
#endif
