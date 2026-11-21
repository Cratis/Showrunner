// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;

namespace Cratis.Showrunner.StageManager.Pulumi.given;

/// <summary>
/// A resource a component registered with the Pulumi engine, as the engine would have received it.
/// </summary>
/// <param name="Type">The type token of the resource.</param>
/// <param name="Name">The Pulumi name of the resource.</param>
/// <param name="Inputs">The inputs of the resource.</param>
public sealed record RecordedResource(string Type, string Name, ImmutableDictionary<string, object> Inputs)
{
    /// <summary>
    /// Gets the Kubernetes name from the metadata of the resource.
    /// </summary>
    public string KubernetesName => (string)Metadata["name"];

    /// <summary>
    /// Gets the namespace from the metadata of the resource.
    /// </summary>
    public string KubernetesNamespace => (string)Metadata["namespace"];

    /// <summary>
    /// Gets the metadata of the resource.
    /// </summary>
    public ImmutableDictionary<string, object> Metadata => (ImmutableDictionary<string, object>)Inputs["metadata"];
}
#endif
