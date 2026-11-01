// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// How the key storage of a <see cref="Production"/> is provided.
/// </summary>
public class ProductionVault
{
    /// <summary>
    /// Gets or sets the mode: <c>None</c>, <c>Managed</c> or <c>External</c>.
    /// </summary>
    [Description("None, Managed or External.")]
    public string Mode { get; set; } = "None";

    /// <summary>
    /// Gets or sets the address of an external Vault.
    /// </summary>
    [Description("The address of an external Vault.")]
    public string Address { get; set; } = string.Empty;
}
