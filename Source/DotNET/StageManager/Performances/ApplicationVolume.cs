// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A volume of the application's pod.
/// </summary>
public class ApplicationVolume
{
    /// <summary>
    /// A scratch directory that lives as long as the pod. Init containers and the application share it.
    /// </summary>
    public const string EmptyDir = "EmptyDir";

    /// <summary>
    /// An existing PersistentVolumeClaim in the namespace of the Performance.
    /// </summary>
    public const string PersistentVolumeClaim = "PersistentVolumeClaim";

    /// <summary>
    /// Gets or sets the name the mounts refer to.
    /// </summary>
    [Description("The name mounts and init containers refer to, as a DNS label such as chronicle-trust.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind: <c>EmptyDir</c> or <c>PersistentVolumeClaim</c>.
    /// </summary>
    [Description("EmptyDir (a scratch directory that lives as long as the pod) or PersistentVolumeClaim (an existing claim named by claimName; Stage Manager never creates or deletes the claim).")]
    public string Kind { get; set; } = EmptyDir;

    /// <summary>
    /// Gets or sets the name of the existing claim.
    /// </summary>
    [Description("For a PersistentVolumeClaim: the name of the existing claim in the namespace of the Performance.")]
    public string ClaimName { get; set; } = string.Empty;
}
