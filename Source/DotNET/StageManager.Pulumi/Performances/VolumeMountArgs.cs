// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// Where a volume of the application's pod is mounted in a container.
/// </summary>
public sealed class VolumeMountArgs
{
    /// <summary>
    /// Gets the name of a volume of the application.
    /// </summary>
    public required string Volume { get; init; }

    /// <summary>
    /// Gets the absolute path the volume is mounted at, such as <c>/var/lib/trust</c>.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets a value indicating whether the volume is mounted read-only.
    /// </summary>
    public bool ReadOnly { get; init; }

    /// <summary>
    /// Builds the spec of the mount.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec().Put("volume", Volume).Put("path", Path).PutWhenTrue("readOnly", ReadOnly);
}
