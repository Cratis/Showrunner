// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What a rollout rolls out: the application version, the runtime image and the package ConfigMap. A change of any of them is a new release.
/// </summary>
/// <param name="Version">The application version.</param>
/// <param name="Image">The Stage runtime image.</param>
/// <param name="PackageConfigMap">The name of the package ConfigMap.</param>
public record PublishedRelease(string Version, string Image, string PackageConfigMap)
{
    /// <summary>
    /// Gets the release a published spec asks for.
    /// </summary>
    /// <param name="spec">The spec of the Performance.</param>
    /// <returns>The release.</returns>
    public static PublishedRelease Of(PerformanceSpec spec) =>
        new(spec.Application.Published!.Version, spec.Application.Image, spec.Application.Published.PackageConfigMapRef.Name);

    /// <summary>
    /// Determines whether a history entry is this release.
    /// </summary>
    /// <param name="record">The history entry.</param>
    /// <returns>True when it is.</returns>
    public bool Is(RolloutRecord record) =>
        string.Equals(record.Version, Version, StringComparison.Ordinal) &&
        string.Equals(record.Image, Image, StringComparison.Ordinal) &&
        string.Equals(record.PackageConfigMap, PackageConfigMap, StringComparison.Ordinal);
}
