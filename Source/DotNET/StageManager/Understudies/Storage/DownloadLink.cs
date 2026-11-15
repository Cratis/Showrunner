// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// A short-lived link to download one object: an S3 presigned URL or an Azure SAS URL.
/// </summary>
/// <param name="Url">The URL. It carries the signature, so it is never logged.</param>
/// <param name="ExpiresAt">When it stops working.</param>
public record DownloadLink(Uri Url, DateTimeOffset ExpiresAt)
{
    /// <summary>
    /// How long a link works when the caller names no time: five minutes.
    /// </summary>
    public static readonly TimeSpan DefaultValidity = TimeSpan.FromMinutes(5);
}
