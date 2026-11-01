// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Labels and owner references shared by everything a Performance owns.
/// </summary>
public static class PerformanceMetadata
{
    /// <summary>
    /// The annotation that carries the revision of the mounted content.
    /// </summary>
    public const string ConfigRevisionAnnotation = "showrunner.cratis.io/config-revision";

    /// <summary>
    /// The label that ties everything to the Performance that owns it.
    /// </summary>
    public const string PerformanceLabel = "showrunner.cratis.io/performance";

    /// <summary>
    /// Gets the labels of a component of a Performance.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="component">The component.</param>
    /// <returns>The labels.</returns>
    public static IDictionary<string, string> Labels(Performance performance, string component) => new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["app.kubernetes.io/name"] = component,
        ["app.kubernetes.io/instance"] = performance.Metadata.Name,
        ["app.kubernetes.io/part-of"] = "showrunner",
        ["app.kubernetes.io/managed-by"] = "stage-manager",
        [PerformanceLabel] = performance.Metadata.Name
    };

    /// <summary>
    /// Creates the metadata of an owned resource. Owned resources are removed with the Performance.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="component">The component it belongs to.</param>
    /// <returns>The metadata.</returns>
    public static V1ObjectMeta Create(Performance performance, string name, string component) => new()
    {
        Name = name,
        NamespaceProperty = performance.Metadata.NamespaceProperty,
        Labels = Labels(performance, component),
        OwnerReferences =
        [
            new V1OwnerReference
            {
                ApiVersion = $"{StageManagerApi.Group}/{StageManagerApi.Version}",
                Kind = "Performance",
                Name = performance.Metadata.Name,
                Uid = performance.Metadata.Uid,
                Controller = true,
                BlockOwnerDeletion = true
            }
        ]
    };
}
