// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text.Json;

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// Signs and verifies configuration snapshots with HMAC-SHA256 under an instance's signing key.
/// </summary>
/// <remarks>
/// The signature covers the canonical form of the whole snapshot. Checks and remedies are ordered by name and secrets by
/// key before serializing, so the form does not depend on the order either side built the lists in.
/// </remarks>
public static class ConfigurationSignature
{
    /// <summary>
    /// The length in bytes of a signing key.
    /// </summary>
    public const int KeyLength = 32;

    /// <summary>
    /// Creates a new random signing key.
    /// </summary>
    /// <returns>The base64 key.</returns>
    public static string NewKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(KeyLength));

    /// <summary>
    /// The canonical bytes of a snapshot - what is signed.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>The UTF-8 JSON bytes.</returns>
    public static byte[] Canonical(WatchdogConfigurationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return JsonSerializer.SerializeToUtf8Bytes(Ordered(snapshot), WatchdogJson.Options);
    }

    /// <summary>
    /// Signs a snapshot. Used by Direct and by specs standing in for it.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="signingKey">The base64 signing key.</param>
    /// <returns>The signed snapshot.</returns>
    /// <exception cref="ArgumentException">The key is not a base64 key of <see cref="KeyLength"/> bytes.</exception>
    public static SignedWatchdogConfiguration Sign(WatchdogConfigurationSnapshot snapshot, string signingKey)
    {
        var key = KeyFrom(signingKey) ?? throw new ArgumentException("The signing key is not a base64 key of the expected length", nameof(signingKey));
        var ordered = Ordered(snapshot);
        return new(ordered, Convert.ToBase64String(HMACSHA256.HashData(key, Canonical(ordered))));
    }

    /// <summary>
    /// Verifies a signed snapshot. Fails closed: a malformed key or signature is not a valid one.
    /// </summary>
    /// <param name="configuration">The signed snapshot.</param>
    /// <param name="signingKey">The base64 signing key.</param>
    /// <returns>True when the signature is the key's signature of the snapshot.</returns>
    public static bool Verify(SignedWatchdogConfiguration? configuration, string signingKey)
    {
        if (configuration?.Snapshot is null || string.IsNullOrEmpty(configuration.Signature) || KeyFrom(signingKey) is not { } key)
        {
            return false;
        }

        byte[] presented;
        try
        {
            presented = Convert.FromBase64String(configuration.Signature);
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = HMACSHA256.HashData(key, Canonical(configuration.Snapshot));
        return CryptographicOperations.FixedTimeEquals(expected, presented);
    }

    /// <summary>
    /// Orders a snapshot's lists canonically.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>The same snapshot with checks and remedies ordered by name and secrets by key.</returns>
    public static WatchdogConfigurationSnapshot Ordered(WatchdogConfigurationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return snapshot with
        {
            Checks = [.. (snapshot.Checks ?? []).OrderBy(check => check.Name, StringComparer.Ordinal)],
            Remedies = [.. (snapshot.Remedies ?? []).OrderBy(remedy => remedy.Name, StringComparer.Ordinal)],
            Secrets = new SortedDictionary<string, string>(
                (snapshot.Secrets ?? new Dictionary<string, string>()).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                StringComparer.Ordinal),
        };
    }

    static byte[]? KeyFrom(string signingKey)
    {
        if (string.IsNullOrEmpty(signingKey))
        {
            return null;
        }

        try
        {
            var key = Convert.FromBase64String(signingKey);
            return key.Length == KeyLength ? key : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
