// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// An HTTP handler that answers from a table of responses by path, and records what it was asked.
/// </summary>
public class StubHttpHandler : HttpMessageHandler
{
    public Dictionary<string, string> Json { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, byte[]> Binary { get; } = new(StringComparer.Ordinal);

    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        var path = request.RequestUri!.AbsolutePath;
        if (Json.TryGetValue(path, out var json))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }

        if (Binary.TryGetValue(path, out var bytes))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
#endif
