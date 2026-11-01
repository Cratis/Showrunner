// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The observed state of a <see cref="Production"/>.
/// </summary>
public class ProductionStatus
{
    /// <summary>
    /// Gets or sets the phase the Production is in.
    /// </summary>
    [Description("The phase the Production is in.")]
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address applications reach Chronicle on.
    /// </summary>
    [Description("The address applications reach Chronicle on.")]
    public string ChronicleAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Secret holding the connection string and client credentials. A Performance consumes it by name and never copies it.
    /// </summary>
    [Description("The name of the Secret holding the connection string and client credentials.")]
    public string ConnectionSecretName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conditions that describe the Production.
    /// </summary>
    [Description("The conditions that describe the Production.")]
    public IList<ResourceCondition> Conditions { get; set; } = [];
}
