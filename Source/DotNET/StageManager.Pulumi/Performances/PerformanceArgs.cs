// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The inputs of a <see cref="Performance"/>.
/// </summary>
public sealed class PerformanceArgs
{
    /// <summary>
    /// Gets the Kubernetes provider the resources are created with, or null for the default one.
    /// </summary>
    public KubernetesProvider? Provider { get; init; }

    /// <summary>
    /// Gets the Kubernetes name of the Performance, or null to use the Pulumi name of the component.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the namespace of the Performance.
    /// </summary>
    public required Input<string> Namespace { get; init; }

    /// <summary>
    /// Gets the name of the Production whose Chronicle the application uses. Give this or <see cref="ChronicleConnectionSecret"/>.
    /// </summary>
    public Input<string>? ProductionName { get; init; }

    /// <summary>
    /// Gets the namespace of the Production, or null for the namespace of the Performance.
    /// </summary>
    public Input<string>? ProductionNamespace { get; init; }

    /// <summary>
    /// Gets the existing Secret holding an explicit Chronicle connection string, used when no Production is named.
    /// </summary>
    public SecretReferenceArgs? ChronicleConnectionSecret { get; init; }

    /// <summary>
    /// Gets the application to run.
    /// </summary>
    public required ApplicationArgs Application { get; init; }

    /// <summary>
    /// Gets the Cratis environment the application runs.
    /// </summary>
    public required PerformanceEnvironmentArgs Environment { get; init; }

    /// <summary>
    /// Gets how long the Performance runs, or null to run permanently.
    /// </summary>
    public LifetimeArgs? Lifetime { get; init; }

    /// <summary>
    /// Gets the labels put on the Performance and on the environment ConfigMap the component writes.
    /// </summary>
    public IDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the resources the Performance and its ConfigMap wait for, such as the namespace and the Production.
    /// </summary>
    public IList<Resource> DependsOn { get; init; } = [];
}
