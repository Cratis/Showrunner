// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Turns the pinned public keys of <see cref="OnBehalfOptions"/> into keys a token can be verified with.
/// </summary>
public static class OnBehalfSigningKeys
{
    /// <summary>
    /// Gets the usable keys: every configured key with an identifier and an RSA public key of at least
    /// <see cref="OnBehalfOptions.MinimumKeySize"/> bits. A key that is not usable verifies nothing.
    /// </summary>
    /// <param name="options">The <see cref="OnBehalfOptions"/>.</param>
    /// <returns>The keys.</returns>
    public static IEnumerable<SecurityKey> From(OnBehalfOptions options) =>
        [.. options.SigningKeys.Select(TryRead).OfType<SecurityKey>()];

    /// <summary>
    /// Gets whether every configured key is usable, so a misconfigured key fails the host at start instead of
    /// refusing every session later.
    /// </summary>
    /// <param name="options">The <see cref="OnBehalfOptions"/>.</param>
    /// <returns>True when every key is usable and no key identifier is used twice.</returns>
    public static bool AreUsable(OnBehalfOptions options) =>
        options.SigningKeys.All(_ => TryRead(_) is not null) &&
        options.SigningKeys.Select(_ => _.KeyId).Distinct(StringComparer.Ordinal).Count() == options.SigningKeys.Count();

    static RsaSecurityKey? TryRead(OnBehalfSigningKey key)
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
            return rsa.KeySize < OnBehalfOptions.MinimumKeySize
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
