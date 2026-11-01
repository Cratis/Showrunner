// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A reference to a Production.
/// </summary>
public class ProductionReference
{
    /// <summary>
    /// Gets or sets the name of the Production.
    /// </summary>
    [Required]
    [Description("The name of the Production.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace of the Production. Defaults to the namespace of the Performance.
    /// </summary>
    [Description("The namespace of the Production. Defaults to the namespace of the Performance.")]
    public string? Namespace { get; set; }
}
