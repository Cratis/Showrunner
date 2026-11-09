// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Labels, owner references and Secrets shared by everything a <see cref="Production"/> owns. Owned resources are removed with the Production.
/// </summary>
public static class ProductionMetadata
{
    /// <summary>
    /// The label that ties everything to the Production that owns it.
    /// </summary>
    public const string ProductionLabel = "showrunner.cratis.io/production";

    /// <summary>
    /// Gets the labels of a component of a Production.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="component">The component.</param>
    /// <returns>The labels.</returns>
    public static IDictionary<string, string> Labels(Production production, string component) => new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["app.kubernetes.io/name"] = component,
        ["app.kubernetes.io/instance"] = production.Metadata.Name,
        ["app.kubernetes.io/part-of"] = "showrunner",
        ["app.kubernetes.io/managed-by"] = "stage-manager",
        [ProductionLabel] = production.Metadata.Name
    };

    /// <summary>
    /// Creates the metadata of an owned resource.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="component">The component it belongs to.</param>
    /// <returns>The metadata.</returns>
    public static V1ObjectMeta Create(Production production, string name, string component) => new()
    {
        Name = name,
        NamespaceProperty = production.Metadata.NamespaceProperty,
        Labels = Labels(production, component),
        OwnerReferences =
        [
            new V1OwnerReference
            {
                ApiVersion = $"{StageManagerApi.Group}/{StageManagerApi.Version}",
                Kind = "Production",
                Name = production.Metadata.Name,
                Uid = production.Metadata.Uid,
                Controller = true,
                BlockOwnerDeletion = true
            }
        ]
    };

    /// <summary>
    /// Creates an Opaque Secret owned by the Production.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="name">The name of the Secret.</param>
    /// <param name="component">The component it belongs to.</param>
    /// <param name="values">The values by key.</param>
    /// <returns>The Secret.</returns>
    public static V1Secret Secret(Production production, string name, string component, IReadOnlyDictionary<string, string> values) => new()
    {
        ApiVersion = "v1",
        Kind = "Secret",
        Metadata = Create(production, name, component),
        Type = "Opaque",
        Data = values.ToDictionary(pair => pair.Key, pair => Encoding.UTF8.GetBytes(pair.Value), StringComparer.Ordinal)
    };
}
