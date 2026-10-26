// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// The public half of a key AuthProxy signs its on-behalf tokens with, pinned in the application's configuration.
/// </summary>
public class OnBehalfSigningKey
{
    /// <summary>
    /// Gets or sets the key identifier, matching the <c>kid</c> header of the tokens it signed and
    /// <c>OnBehalf:Attestation:SigningKeys:KeyId</c> in AuthProxy.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PEM-encoded RSA public key, of at least 2048 bits. It is not a secret.
    /// </summary>
    public string PublicKeyPem { get; set; } = string.Empty;
}
