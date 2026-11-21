// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.ApiExtensions;

namespace Cratis.Showrunner.StageManager.Pulumi;

/// <summary>
/// The inputs of a <see cref="StageManagerCustomResource"/>: the metadata and the spec as plain values.
/// </summary>
/// <param name="kind">The kind of the custom resource.</param>
internal sealed class StageManagerCustomResourceArgs(string kind) : CustomResourceArgs(ShowrunnerApi.ApiVersion, kind)
{
    /// <summary>
    /// Gets or sets the spec. Nested values may be inputs and outputs; Pulumi resolves them.
    /// </summary>
    [Input("spec")]
    public Input<object>? Spec { get; set; }
}
