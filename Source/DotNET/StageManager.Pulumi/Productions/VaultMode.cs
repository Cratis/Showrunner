// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// How the key storage of a Production is provided.
/// </summary>
public enum VaultMode
{
    /// <summary>
    /// Chronicle keeps its keys in its database.
    /// </summary>
    None = 0,

    /// <summary>
    /// Stage Manager runs Vault.
    /// </summary>
    Managed = 1,

    /// <summary>
    /// A Vault that something else runs.
    /// </summary>
    External = 2
}
