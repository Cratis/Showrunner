// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The public half of a key that used to sign, kept pinned while tokens signed with it can still be valid.
/// </summary>
/// <param name="KeyId">The id of the key.</param>
/// <param name="PublicKeyPem">The public key in PEM.</param>
public record PublicKeyMaterial(string KeyId, string PublicKeyPem);
