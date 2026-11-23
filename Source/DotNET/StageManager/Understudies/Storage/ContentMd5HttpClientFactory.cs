// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Amazon.Runtime;

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// Creates the HTTP clients of an S3 client so that the requests some S3 services insist on a <c>Content-MD5</c> header for carry one.
/// </summary>
/// <remarks>
/// AWSSDK.S3 sends a CRC32 checksum for a bucket lifecycle configuration and no <c>Content-MD5</c>, which services such as UpCloud Managed Object Storage refuse.
/// </remarks>
public sealed class ContentMd5HttpClientFactory : HttpClientFactory
{
    /// <inheritdoc/>
    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
#pragma warning disable CA2000 // The S3 client owns the HttpClient it asks for and disposes it, and the handler with it.
        return new(new ContentMd5Handler(new HttpClientHandler()));
#pragma warning restore CA2000
    }
}
