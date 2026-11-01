// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Generates and rotates the signing keys of a Performance. Rotation moves the public half of the key in use to the pinned previous keys, so tokens signed before still verify.
/// </summary>
public static class SigningKeys
{
    const int KeySizeInBits = 3072;

    /// <summary>
    /// Makes sure a Performance has signing keys, rotating them when the rotation counter was raised.
    /// </summary>
    /// <param name="existing">The state that is stored, or null when there is none.</param>
    /// <param name="rotation">The rotation counter in the spec.</param>
    /// <param name="retainPrevious">How many previous public keys stay pinned.</param>
    /// <returns>The state to store.</returns>
    public static SigningKeyState Ensure(SigningKeyState? existing, int rotation, int retainPrevious)
    {
        var retain = Math.Max(0, retainPrevious);
        if (existing is null)
        {
            return new(new(Generate(), []), new(Generate(), []), rotation);
        }

        if (rotation <= existing.Rotation)
        {
            return existing with
            {
                Invitation = Trim(existing.Invitation, retain),
                Attestation = Trim(existing.Attestation, retain)
            };
        }

        return new(Rotate(existing.Invitation, retain), Rotate(existing.Attestation, retain), rotation);
    }

    /// <summary>
    /// Generates a new RSA key pair.
    /// </summary>
    /// <returns>The key pair.</returns>
    public static SigningKeyMaterial Generate()
    {
        using var rsa = RSA.Create(KeySizeInBits);
        var parameters = rsa.ExportParameters(false);
        var thumbprint = $$"""{"e":"{{Base64Url(parameters.Exponent!)}}","kty":"RSA","n":"{{Base64Url(parameters.Modulus!)}}"}""";
        var keyId = Base64Url(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(thumbprint)));
        return new(keyId, rsa.ExportPkcs8PrivateKeyPem(), rsa.ExportSubjectPublicKeyInfoPem());
    }

    static KeyRing Rotate(KeyRing ring, int retain) =>
        Trim(new(Generate(), [new(ring.Current.KeyId, ring.Current.PublicKeyPem), .. ring.Previous]), retain);

    static KeyRing Trim(KeyRing ring, int retain) =>
        ring.Previous.Count <= retain ? ring : ring with { Previous = [.. ring.Previous.Take(retain)] };

    static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
