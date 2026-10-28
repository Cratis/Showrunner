// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using Cratis.Arc.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Turns the public keys pinned in Arc's <see cref="OnBehalfOfOptions"/> into keys AuthProxy's call tokens can be
/// verified with.
/// </summary>
public static class OnBehalfSigningKeys
{
    /// <summary>
    /// The smallest RSA key Usher accepts tokens signed with.
    /// </summary>
    public const int MinimumKeySize = 2048;

    /// <summary>
    /// Gets the usable keys: every configured key with an identifier and an RSA public key of at least
    /// <see cref="MinimumKeySize"/> bits. A key that is not usable verifies nothing.
    /// </summary>
    /// <param name="keys">The configured <see cref="OnBehalfOfSigningKey">keys</see>.</param>
    /// <returns>The keys.</returns>
    public static IEnumerable<SecurityKey> From(IEnumerable<OnBehalfOfSigningKey> keys) =>
        [.. keys.Select(TryRead).OfType<SecurityKey>()];

    /// <summary>
    /// Gets whether every configured key is usable, so a misconfigured key fails the host at start instead of
    /// refusing every session later.
    /// </summary>
    /// <param name="keys">The configured <see cref="OnBehalfOfSigningKey">keys</see>.</param>
    /// <returns>True when every key is usable and no key identifier is used twice.</returns>
    public static bool AreUsable(IEnumerable<OnBehalfOfSigningKey> keys) =>
        keys.All(_ => TryRead(_) is not null) &&
        keys.Select(_ => _.KeyId).Distinct(StringComparer.Ordinal).Count() == keys.Count();

    static RsaSecurityKey? TryRead(OnBehalfOfSigningKey key)
    {
        // A private key has no place in the application: refusing it keeps AuthProxy's signing key from being
        // configured, and spread, where only its public half is needed.
        if (string.IsNullOrWhiteSpace(key.KeyId) || string.IsNullOrWhiteSpace(key.PublicKeyPem) ||
            key.PublicKeyPem.Contains("PRIVATE KEY", StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(key.PublicKeyPem);
            return rsa.KeySize < MinimumKeySize
                ? null
                : new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = key.KeyId };
        }
        catch (Exception exception) when (exception is CryptographicException or ArgumentException)
        {
            // An unreadable key is not a key: it verifies nothing, and the options validation reports it at start.
            return null;
        }
    }
}
