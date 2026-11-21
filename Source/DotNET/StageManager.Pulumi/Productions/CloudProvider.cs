// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The cloud whose key service unseals a managed Vault.
/// </summary>
public enum CloudProvider
{
    /// <summary>
    /// AWS KMS.
    /// </summary>
    Aws = 0,

    /// <summary>
    /// Azure Key Vault.
    /// </summary>
    Azure = 1,

    /// <summary>
    /// Google Cloud KMS.
    /// </summary>
    Google = 2
}
