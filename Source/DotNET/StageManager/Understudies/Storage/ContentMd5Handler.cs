// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// Adds the <c>Content-MD5</c> header to the bucket lifecycle configuration request, which some S3 services refuse without.
/// </summary>
/// <param name="inner">The handler that sends the request.</param>
public sealed class ContentMd5Handler(HttpMessageHandler inner) : DelegatingHandler(inner)
{
    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (NeedsContentMd5(request))
        {
            var body = await request.Content!.ReadAsByteArrayAsync(cancellationToken);

            // Content-MD5 is the integrity header the service demands here; it is not used for anything security relevant.
#pragma warning disable CA5351
            request.Content.Headers.ContentMD5 = MD5.HashData(body);
#pragma warning restore CA5351
        }

        return await base.SendAsync(request, cancellationToken);
    }

    static bool NeedsContentMd5(HttpRequestMessage request) =>
        request.Method == HttpMethod.Put &&
        request.Content is { } content &&
        content.Headers.ContentMD5 is null &&
        request.RequestUri is { } uri &&
        uri.Query.Contains("lifecycle", StringComparison.Ordinal);
}
