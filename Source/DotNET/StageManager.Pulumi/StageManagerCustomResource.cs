// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using KubernetesCustomResource = Pulumi.Kubernetes.ApiExtensions.CustomResource;

namespace Cratis.Showrunner.StageManager.Pulumi;

/// <summary>
/// A custom resource of the <c>showrunner.cratis.io</c> group, declared with its spec as plain values.
/// </summary>
/// <param name="name">The Pulumi name.</param>
/// <param name="args">The inputs.</param>
/// <param name="options">The resource options.</param>
internal sealed class StageManagerCustomResource(string name, StageManagerCustomResourceArgs args, CustomResourceOptions options)
    : KubernetesCustomResource(name, args, options);
