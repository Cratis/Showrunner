// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Keys;

/// <summary>
/// One secret of Vault, with its values. It is plaintext and has no printable form on purpose.
/// </summary>
public class VaultSecret
{
    /// <summary>
    /// Gets the path of the secret under the mount.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the values of the secret.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Data { get; init; }
}
