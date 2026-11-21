// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text.Json;

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The SHA-256 digest of a configuration's content - checks, remedies, secrets and offline policy - independent of
/// its revision and times.
/// </summary>
public static class ConfigurationDigest
{
    /// <summary>
    /// Computes the digest of a snapshot's content.
    /// </summary>
    /// <param name="snapshot">The snapshot; its revision, times and existing digest are ignored.</param>
    /// <returns>The lowercase hex digest.</returns>
    public static string Of(WatchdogConfigurationSnapshot snapshot)
    {
        var ordered = ConfigurationSignature.Ordered(snapshot);
        var content = new { ordered.Checks, ordered.Remedies, ordered.Secrets, ordered.Offline };
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(content, WatchdogJson.Options))).ToLowerInvariant();
    }
}
