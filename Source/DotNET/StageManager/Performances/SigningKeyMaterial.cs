// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A signing key pair. The private key never leaves a Secret.
/// </summary>
/// <param name="KeyId">The id of the key, its RFC 7638 thumbprint.</param>
/// <param name="PrivateKeyPem">The private key in PEM.</param>
/// <param name="PublicKeyPem">The public key in PEM.</param>
public record SigningKeyMaterial(string KeyId, string PrivateKeyPem, string PublicKeyPem);
