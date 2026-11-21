// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// A volume of the application's pod.
/// </summary>
public sealed class VolumeArgs
{
    /// <summary>
    /// Gets the name mounts and init containers refer to, as a DNS label such as <c>chronicle-trust</c>.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the kind. Defaults to <see cref="VolumeKind.EmptyDir"/>.
    /// </summary>
    public VolumeKind Kind { get; init; } = VolumeKind.EmptyDir;

    /// <summary>
    /// Gets the name of the existing claim, for <see cref="VolumeKind.PersistentVolumeClaim"/>.
    /// </summary>
    public string? ClaimName { get; init; }

    /// <summary>
    /// Builds the spec of the volume.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec().Put("name", Name).PutEnum("kind", Kind).Put("claimName", ClaimName);
}
