// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The observed state of a <see cref="Performance"/>. It never contains a secret value.
/// </summary>
public class PerformanceStatus
{
    /// <summary>
    /// Gets or sets the phase.
    /// </summary>
    [Description("Pending, Blocked, Progressing or Ready.")]
    public PerformancePhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the public URL of the application, through AuthProxy.
    /// </summary>
    [Description("The public URL of the application, through AuthProxy.")]
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment keys the Performance needs and the environment does not have.
    /// </summary>
    [Description("The environment keys the Performance needs and the environment does not have. While this is not empty nothing is rolled out.")]
    public IList<string> MissingEnvironmentKeys { get; set; } = [];

    /// <summary>
    /// Gets or sets the generation of the spec these conditions describe.
    /// </summary>
    [Description("The generation of the spec these conditions describe.")]
    public long ObservedGeneration { get; set; }

    /// <summary>
    /// Gets or sets the state of the generated signing keys.
    /// </summary>
    [Description("The state of the generated signing keys.")]
    public SigningKeysStatus SigningKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets the conditions, one per component and one per precondition.
    /// </summary>
    [Description("One condition per component (Application, AuthProxy, Ante) and per precondition.")]
    public IList<ResourceCondition> Conditions { get; set; } = [];
}
