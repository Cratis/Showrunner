// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The tier of a Vault that Stage Manager runs.
/// </summary>
public enum VaultTier
{
    /// <summary>
    /// One node with file storage, where Chronicle uses the root token.
    /// </summary>
    Development = 0,

    /// <summary>
    /// Three nodes with Raft storage, where Chronicle uses a token that reaches only its own key paths.
    /// </summary>
    Production = 1
}
