// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using k8s;
using k8s.Models;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the <c>StageApplication</c> custom resource, <c>showrunner.cratis.io/v1alpha1</c>.</summary>
[KubernetesEntity(Group = Group, ApiVersion = Version, Kind = ResourceKind, PluralName = Plural)]
public class StageApplication : IKubernetesObject<V1ObjectMeta>
{
    /// <summary>The API group.</summary>
    public const string Group = "showrunner.cratis.io";

    /// <summary>The API version.</summary>
    public const string Version = "v1alpha1";

    /// <summary>The kind.</summary>
    public const string ResourceKind = "StageApplication";

    /// <summary>The plural resource name.</summary>
    public const string Plural = "stageapplications";

    /// <summary>The finalizer that holds a deleted resource until its data retention has passed.</summary>
    public const string RetentionFinalizer = "showrunner.cratis.io/data-retention";

    /// <inheritdoc/>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = $"{Group}/{Version}";

    /// <inheritdoc/>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = ResourceKind;

    /// <inheritdoc/>
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = new();

    /// <summary>Gets or sets the desired state.</summary>
    [JsonPropertyName("spec")]
    public StageApplicationSpec Spec { get; set; } = new();

    /// <summary>Gets or sets the observed state.</summary>
    [JsonPropertyName("status")]
    public StageApplicationStatus? Status { get; set; }

    /// <summary>Gets the namespace of the resource, or empty.</summary>
    public string Namespace => Metadata?.NamespaceProperty ?? string.Empty;

    /// <summary>Gets the name of the resource, or empty.</summary>
    public string Name => Metadata?.Name ?? string.Empty;

    /// <summary>Gets a value indicating whether the resource is being deleted.</summary>
    public bool IsDeleting => Metadata?.DeletionTimestamp is not null;
}
