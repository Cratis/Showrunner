// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// Writes a <see cref="ReleaseAsset"/>, answering a repeat request whose tag still matches with 304 Not Modified.
/// </summary>
/// <param name="asset">The file.</param>
/// <param name="tag">The tag that identifies this content for as long as the build lives.</param>
public class CachedFileResult(ReleaseAsset asset, EntityTagHeaderValue tag) : IResult
{
    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;

        // Private, because the endpoints are for signed-in users unless the application says otherwise.
        response.Headers.CacheControl = "private, max-age=31536000, immutable";
        response.Headers.ETag = tag.ToString();

        var match = httpContext.Request.GetTypedHeaders().IfNoneMatch;
        if (match.Any(candidate => candidate.Equals(EntityTagHeaderValue.Any) || candidate.Compare(tag, useStrongComparison: false)))
        {
            response.StatusCode = StatusCodes.Status304NotModified;
            return;
        }

        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = asset.ContentType;
        await using var stream = asset.Open();
        response.ContentLength = stream.Length;
        await stream.CopyToAsync(response.Body, httpContext.RequestAborted);
    }
}
